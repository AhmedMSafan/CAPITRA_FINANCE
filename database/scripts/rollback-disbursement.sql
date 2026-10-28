-- =====================================================================
-- Capitra: roll back the Disbursement step of ONE purchased portfolio,
-- so it can be done again (TEST data only).
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5.
--
-- Set v_portfolio_no below to the portfolio number you see on screen.
-- It undoes everything the Disbursement "Complete" button creates:
--   1. the disbursement row, repayment schedule and income schedule
--   2. the wallet cheques (CHQ_CHEQUES) and their events
--   3. the CRM clients the disbursement created (kept if used elsewhere)
--   4. the GL batch and journals of the TRAN_SET_ID; posted journals are
--      also taken out of GL_ACCOUNT_BALANCES and GL_DAILY_BALANCES
--   5. the workflow: the Disbursement task is open and unclaimed again
--   6. the portfolio status (back to the status before disbursement)
-- Everything runs in one transaction: any error changes nothing.
-- The investor cheques of the portfolio keep the status RECEIVED.
-- =====================================================================

-- helper for this session only: deletes the rows of p_table that match p_where,
-- after deleting every row of another table that points at them (found from the foreign keys)
CREATE OR REPLACE FUNCTION pg_temp.capitra_purge(p_table text, p_where text, p_path text[] DEFAULT '{}') RETURNS void
LANGUAGE plpgsql AS $f$
DECLARE
  fk  record;
  n   bigint;
  hit boolean;
  sub text;
BEGIN
  FOR fk IN
    SELECT ct.relname AS child, ca.attname AS child_col, pa.attname AS parent_col
      FROM pg_constraint k
      JOIN pg_class ct ON ct.oid = k.conrelid
      JOIN pg_class pt ON pt.oid = k.confrelid
      JOIN pg_attribute ca ON ca.attrelid = k.conrelid AND ca.attnum = k.conkey[1]
      JOIN pg_attribute pa ON pa.attrelid = k.confrelid AND pa.attnum = k.confkey[1]
     WHERE k.contype = 'f' AND pt.relname = p_table AND pt.relnamespace = 'public'::regnamespace
  LOOP
    sub := format('%I IN (SELECT %I FROM %I WHERE %s)', fk.child_col, fk.parent_col, p_table, p_where);
    EXECUTE format('SELECT EXISTS (SELECT 1 FROM %I WHERE %s)', fk.child, sub) INTO hit;
    IF NOT hit THEN
      CONTINUE;
    ELSIF fk.child = p_table OR fk.child = ANY (p_path) THEN
      -- a row of a table already being cleared (a reversal of a journal) only loses the link
      EXECUTE format('UPDATE %I SET %I = NULL WHERE %s', fk.child, fk.child_col, sub);
    ELSE
      PERFORM pg_temp.capitra_purge(fk.child, sub, p_path || p_table);
    END IF;
  END LOOP;
  EXECUTE format('DELETE FROM %I WHERE %s', p_table, p_where);
  GET DIAGNOSTICS n = ROW_COUNT;
  IF n > 0 THEN
    RAISE NOTICE '% rows deleted from %', n, p_table;
  END IF;
END
$f$;

DO $$
DECLARE
  v_portfolio_no  text    := '1200';   -- PORTFOLIO_NO to roll back
  v_allow_posted  boolean := true;     -- false: stop if the GL journals are already posted

  v_pf     bigint;
  v_disb   bigint;
  v_ts     text;
  v_task   bigint;
  v_sid    bigint;
  v_inst   bigint;
  v_elem   text;
  v_batch  bigint;
  v_prev   text;
  v_hist   bigint;
  v_n      bigint;
  v_chq    bigint[];
  v_cli    bigint[];
  c        bigint;
BEGIN
  -- the portfolio and what its disbursement created
  SELECT "PORTFOLIO_ID" INTO v_pf FROM "MTG_PURCHASED_PORTFOLIOS" WHERE "PORTFOLIO_NO" = v_portfolio_no;
  IF v_pf IS NULL THEN
    RAISE EXCEPTION 'Portfolio % was not found. Nothing changed.', v_portfolio_no;
  END IF;
  SELECT "DISBURSEMENT_ID", "TRAN_SET_ID", "WORKFLOW_TASK_ID" INTO v_disb, v_ts, v_task
    FROM "MTG_PURCHASED_PORTFOLIO_DISBURSEMENTS" WHERE "PORTFOLIO_ID" = v_pf;
  IF v_disb IS NULL THEN
    RAISE EXCEPTION 'Portfolio % has no disbursement. Nothing changed.', v_portfolio_no;
  END IF;
  RAISE NOTICE 'Portfolio % (id %): disbursement %, TRAN_SET_ID %, task %', v_portfolio_no, v_pf, v_disb, v_ts, v_task;

  SELECT array_agg(DISTINCT "WALLET_CHEQUE_ID") INTO v_chq
    FROM "MTG_PURCHASED_PORTFOLIO_REPAYMENT_SCHEDULES" WHERE "DISBURSEMENT_ID" = v_disb AND "WALLET_CHEQUE_ID" IS NOT NULL;
  SELECT array_agg(DISTINCT "CLIENT_ID") INTO v_cli
    FROM "MTG_PURCHASED_PORTFOLIO_REPAYMENT_SCHEDULES" WHERE "DISBURSEMENT_ID" = v_disb AND "IS_CLIENT_CREATED" AND "CLIENT_ID" IS NOT NULL;

  -- 1. schedules and the disbursement row
  DELETE FROM "MTG_PURCHASED_PORTFOLIO_INCOME_SCHEDULES" WHERE "DISBURSEMENT_ID" = v_disb;
  GET DIAGNOSTICS v_n = ROW_COUNT; RAISE NOTICE '% income schedule rows deleted', v_n;
  DELETE FROM "MTG_PURCHASED_PORTFOLIO_REPAYMENT_SCHEDULES" WHERE "DISBURSEMENT_ID" = v_disb;
  GET DIAGNOSTICS v_n = ROW_COUNT; RAISE NOTICE '% repayment schedule rows deleted', v_n;
  DELETE FROM "MTG_PURCHASED_PORTFOLIO_DISBURSEMENTS" WHERE "DISBURSEMENT_ID" = v_disb;
  RAISE NOTICE 'disbursement row deleted';

  -- 2. wallet cheques and their events
  IF v_chq IS NOT NULL THEN
    UPDATE "CHQ_CHEQUES" SET "LAST_EVENT_ID" = NULL WHERE "CHEQUE_ID" = ANY (v_chq);
    DELETE FROM "CHQ_EVENT_GROUPS" WHERE "MOVEMENT_GROUP_ID" IN
      (SELECT "MOVEMENT_GROUP_ID" FROM "CHQ_EVENTS" WHERE "CHEQUE_ID" = ANY (v_chq) AND "MOVEMENT_GROUP_ID" IS NOT NULL)
      AND NOT EXISTS (SELECT 1 FROM "CHQ_EVENTS" e WHERE e."MOVEMENT_GROUP_ID" = "CHQ_EVENT_GROUPS"."MOVEMENT_GROUP_ID" AND NOT e."CHEQUE_ID" = ANY (v_chq));
    DELETE FROM "CHQ_BANK_CHARGES" WHERE "CHEQUE_ID" = ANY (v_chq);
    DELETE FROM "CHQ_EVENTS" WHERE "CHEQUE_ID" = ANY (v_chq);
    DELETE FROM "CHQ_CHEQUES" WHERE "CHEQUE_ID" = ANY (v_chq);
    RAISE NOTICE '% wallet cheques deleted', cardinality(v_chq);
  END IF;

  -- 3. CRM clients created by this disbursement (kept when another record uses them)
  IF v_cli IS NOT NULL THEN
    FOREACH c IN ARRAY v_cli LOOP
      BEGIN
        DELETE FROM "CRM_CLIENTS" WHERE "CLIENT_ID" = c AND "ATTRIBUTE1" LIKE 'FNV_INVESTOR:%';
        RAISE NOTICE 'client % deleted', c;
      EXCEPTION WHEN foreign_key_violation THEN
        RAISE NOTICE 'client % kept (used elsewhere); the next disbursement reuses it', c;
      END;
    END LOOP;
  END IF;

  -- 4. GL batch and journals of the TRAN_SET_ID
  SELECT max(b."BATCH_ID") INTO v_batch
    FROM "GL_JOURNAL_BATCHES" b JOIN "GL_JOURNAL_SOURCES" s ON s."SOURCE_ID" = b."SOURCE_ID"
   WHERE b."SOURCE_BATCH_REF" = v_ts AND s."SOURCE_CODE" = 'PURCHASED_PORTFOLIO';
  IF v_batch IS NULL THEN
    RAISE NOTICE 'no GL batch found for %', v_ts;
  ELSE
    SELECT count(*) INTO v_n FROM "GL_JOURNAL_HEADERS" WHERE "BATCH_ID" = v_batch AND "POSTING_STATUS" = 'POSTED';
    IF v_n > 0 AND NOT v_allow_posted THEN
      RAISE EXCEPTION 'The GL journals of % are already POSTED. Set v_allow_posted to true to remove them anyway (test only). Nothing changed.', v_ts;
    END IF;

    -- take posted amounts out of the balances (posting adds each line to its period and day)
    UPDATE "GL_ACCOUNT_BALANCES" ab
       SET "PERIOD_DR" = ab."PERIOD_DR" - i.dr, "PERIOD_CR" = ab."PERIOD_CR" - i.cr,
           "CLOSING_DR" = ab."CLOSING_DR" - i.dr, "CLOSING_CR" = ab."CLOSING_CR" - i.cr
      FROM (SELECT "LEDGER_ID", "LEDGER_PERIOD_ID", "ACCOUNT_COMBINATION_ID", "CURRENCY_CODE",
                   sum("ACCOUNTED_DR") dr, sum("ACCOUNTED_CR") cr
              FROM "GL_POSTING_BALANCE_IMPACTS" WHERE "JOURNAL_BATCH_ID" = v_batch
             GROUP BY 1, 2, 3, 4) i
     WHERE ab."LEDGER_ID" = i."LEDGER_ID" AND ab."LEDGER_PERIOD_ID" = i."LEDGER_PERIOD_ID"
       AND ab."ACCOUNT_COMBINATION_ID" = i."ACCOUNT_COMBINATION_ID" AND ab."CURRENCY_CODE" = i."CURRENCY_CODE";
    GET DIAGNOSTICS v_n = ROW_COUNT; RAISE NOTICE '% account balance rows reduced', v_n;
    UPDATE "GL_DAILY_BALANCES" d
       SET "DAILY_DR" = d."DAILY_DR" - i.dr, "DAILY_CR" = d."DAILY_CR" - i.cr,
           "CLOSING_DR" = d."CLOSING_DR" - i.dr, "CLOSING_CR" = d."CLOSING_CR" - i.cr
      FROM (SELECT "LEDGER_ID", "ACCOUNTING_DATE", "ACCOUNT_COMBINATION_ID", "CURRENCY_CODE",
                   sum("ACCOUNTED_DR") dr, sum("ACCOUNTED_CR") cr
              FROM "GL_POSTING_BALANCE_IMPACTS" WHERE "JOURNAL_BATCH_ID" = v_batch
             GROUP BY 1, 2, 3, 4) i
     WHERE d."LEDGER_ID" = i."LEDGER_ID" AND d."BALANCE_DATE" = i."ACCOUNTING_DATE"
       AND d."ACCOUNT_COMBINATION_ID" = i."ACCOUNT_COMBINATION_ID" AND d."CURRENCY_CODE" = i."CURRENCY_CODE";
    GET DIAGNOSTICS v_n = ROW_COUNT; RAISE NOTICE '% daily balance rows reduced', v_n;
    UPDATE "GL_ACCOUNT_BALANCES" SET "LAST_POSTED_HEADER_ID" = NULL
     WHERE "LAST_POSTED_HEADER_ID" IN (SELECT "JOURNAL_HEADER_ID" FROM "GL_JOURNAL_HEADERS" WHERE "BATCH_ID" = v_batch);

    -- lines, journals and the batch, each after every row that points at them (found from the foreign keys)
    PERFORM pg_temp.capitra_purge('GL_JOURNAL_LINES',
      format('"JOURNAL_HEADER_ID" IN (SELECT "JOURNAL_HEADER_ID" FROM "GL_JOURNAL_HEADERS" WHERE "BATCH_ID" = %s)', v_batch));
    PERFORM pg_temp.capitra_purge('GL_JOURNAL_HEADERS', format('"BATCH_ID" = %s', v_batch));
    PERFORM pg_temp.capitra_purge('GL_JOURNAL_BATCHES', format('"BATCH_ID" = %s', v_batch));
    RAISE NOTICE 'GL batch % deleted', v_batch;
  END IF;

  -- 5. workflow: the Disbursement task open and unclaimed again
  SELECT t."STEP_INSTANCE_ID", s."WORKFLOW_INSTANCE_ID", s."ELEMENT_ID" INTO v_sid, v_inst, v_elem
    FROM "WFE_TASKS" t JOIN "WFE_STEP_INSTANCES" s ON s."STEP_INSTANCE_ID" = t."STEP_INSTANCE_ID"
   WHERE t."TASK_ID" = v_task;
  IF v_sid IS NULL THEN
    RAISE EXCEPTION 'Workflow task % of the disbursement was not found. Nothing changed.', v_task;
  END IF;

  -- steps the workflow entered after the Disbursement (e.g. End)
  DELETE FROM "WFE_INBOX" WHERE "STEP_INSTANCE_ID" IN
    (SELECT "STEP_INSTANCE_ID" FROM "WFE_STEP_INSTANCES" WHERE "WORKFLOW_INSTANCE_ID" = v_inst AND "STEP_INSTANCE_ID" > v_sid);
  DELETE FROM "WFE_TASKS" WHERE "STEP_INSTANCE_ID" IN
    (SELECT "STEP_INSTANCE_ID" FROM "WFE_STEP_INSTANCES" WHERE "WORKFLOW_INSTANCE_ID" = v_inst AND "STEP_INSTANCE_ID" > v_sid);
  DELETE FROM "WFE_STEP_INSTANCES" WHERE "WORKFLOW_INSTANCE_ID" = v_inst AND "STEP_INSTANCE_ID" > v_sid;
  GET DIAGNOSTICS v_n = ROW_COUNT; RAISE NOTICE '% later workflow steps removed', v_n;

  UPDATE "WFE_TASKS" SET "TASK_STATUS" = 'OPEN', "CLAIMED_BY" = NULL, "CLAIMED_AT" = NULL, "COMPLETED_AT" = NULL,
         "UPDATED_BY" = 'ROLLBACK', "UPDATED_AT" = now(), "ROW_VERSION" = "ROW_VERSION" + 1
   WHERE "TASK_ID" = v_task;
  UPDATE "WFE_INBOX" SET "INBOX_STATUS" = 'OPEN', "CLAIMED_BY" = NULL, "CLAIMED_AT" = NULL, "COMPLETED_AT" = NULL, "OUTCOME" = NULL,
         "UPDATED_BY" = 'ROLLBACK', "UPDATED_AT" = now(), "ROW_VERSION" = "ROW_VERSION" + 1
   WHERE "TASK_ID" = v_task;
  UPDATE "WFE_STEP_INSTANCES" SET "STEP_STATUS" = 'WAITING', "COMPLETED_AT" = NULL, "COMPLETED_BY" = NULL, "OUTCOME" = NULL,
         "UPDATED_BY" = 'ROLLBACK', "UPDATED_AT" = now(), "ROW_VERSION" = "ROW_VERSION" + 1
   WHERE "STEP_INSTANCE_ID" = v_sid;
  UPDATE "WFE_WORKFLOW_INSTANCES" SET "INSTANCE_STATUS" = 'WAITING', "CURRENT_ELEMENT_ID" = v_elem, "COMPLETED_AT" = NULL, "ERROR_MESSAGE" = NULL,
         "UPDATED_BY" = 'ROLLBACK', "UPDATED_AT" = now(), "ROW_VERSION" = "ROW_VERSION" + 1
   WHERE "WORKFLOW_INSTANCE_ID" = v_inst;

  -- history from the claim of the Disbursement task onwards
  SELECT max("INSTANCE_HIST_ID") INTO v_hist FROM "WFE_INSTANCE_HIST"
   WHERE "WORKFLOW_INSTANCE_ID" = v_inst AND "EVENT_TYPE" = 'TASK_CREATED' AND "ELEMENT_ID" = v_elem;
  IF v_hist IS NOT NULL THEN
    DELETE FROM "WFE_INSTANCE_HIST" WHERE "WORKFLOW_INSTANCE_ID" = v_inst AND "INSTANCE_HIST_ID" > v_hist;
  END IF;
  RAISE NOTICE 'workflow back on % (task % open, not claimed)', v_elem, v_task;

  -- 6. portfolio status before the disbursement
  SELECT "FROM_STATUS" INTO v_prev FROM "MTG_PURCHASED_PORTFOLIO_STATUS_HIST"
   WHERE "PORTFOLIO_ID" = v_pf AND "TO_STATUS" = 'DISBURSEMENT_COMPLETED'
   ORDER BY "STATUS_HIST_ID" DESC LIMIT 1;
  v_prev := coalesce(v_prev, 'BOARD_REVIEW_ACCEPTED');
  UPDATE "MTG_PURCHASED_PORTFOLIOS" SET "PORTFOLIO_STATUS_CODE" = v_prev, "WORKFLOW_TASK_ID" = v_task,
         "UPDATED_BY" = 'ROLLBACK', "UPDATED_AT" = now(), "ROW_VERSION" = "ROW_VERSION" + 1
   WHERE "PORTFOLIO_ID" = v_pf;
  DELETE FROM "MTG_PURCHASED_PORTFOLIO_STATUS_HIST" WHERE "PORTFOLIO_ID" = v_pf AND "TO_STATUS" = 'DISBURSEMENT_COMPLETED';
  RAISE NOTICE 'portfolio back to %', v_prev;

  RAISE NOTICE 'Rollback done. Open the Disbursement task from the inbox and complete it again.';
END $$;
