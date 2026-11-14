-- The queries of the on-demand processes of App 122 (Cheques Wallet), as the PL/SQL ran them.
-- LKP_CURRENCY is FIN_CURRENCIES, FND_LEGAL_ENTITY is FIN_LEGAL_ENTITIES and FND_BANK_ACCOUNT is TRS_BANK_ACCOUNTS in Capitra.

-- name: STATUS_CODES
select status_code from lkp_cheque_status order by display_order, status_code

-- name: STATUSES
select status_code, name_en from lkp_cheque_status order by display_order, status_code

-- name: STATUSES_ANY
select status_code, name_en from lkp_cheque_status

-- name: LOCATIONS_ACTIVE
select location_code, location_name from lkp_cheque_location where active_flag = 'Y' order by display_seq, location_name

-- name: LOCATIONS
select location_code, location_name from lkp_cheque_location order by display_seq, location_name

-- name: LOCATIONS_ANY
select location_code, location_name from lkp_cheque_location

-- name: CURRENCIES
select currency_code, currency_name_en name_en from fin_currencies order by currency_name_en

-- name: WALLET
select c.cheque_id, c.cheque_ref, c.cheque_no, c.customer_id, c.customer_name_en, c.drawer_name_en, c.issuer_bank_code, c.issuer_bank_name,
       c.due_date, c.cheque_amount, c.currency_code, c.status_code, s.name_en status_name, s.is_terminal,
       c.current_location_code, l.location_name, c.current_custodian_user, b.batch_no
  from fnd_cheque c
  left join lkp_cheque_status s on s.status_code = c.status_code
  left join lkp_cheque_location l on l.location_code = c.current_location_code
  left join fnd_cheque_upload_batch b on b.batch_id = c.batch_id
 order by c.due_date, c.cheque_id
 fetch first 3000 rows only

-- name: BATCHES
select b.batch_id, b.batch_no, b.batch_status, b.file_name, b.customer_id, b.customer_name_en, b.customer_name_ar,
       b.currency_code, b.total_rows, b.valid_rows, b.error_rows, b.submitted_rows, b.notes,
       b.created_by, b.created_dt, b.submitted_by, b.submitted_dt,
       (select sum(l.cheque_amount) from fnd_cheque_upload_line l where l.batch_id = b.batch_id) amount
  from fnd_cheque_upload_batch b
 order by b.created_dt desc nulls last, b.batch_id desc
 fetch first 1001 rows only

-- name: BATCH
select b.batch_id, b.batch_no, b.batch_status, b.file_name, b.customer_id, b.customer_name_en, b.customer_name_ar, b.currency_code,
       b.default_location_code, b.source_module, b.notes, b.total_rows, b.valid_rows, b.error_rows, b.submitted_rows,
       b.received_dt, b.cheque_date, b.created_dt, b.created_by, b.updated_dt, b.updated_by, b.submitted_dt, b.submitted_by,
       (select sum(l.cheque_amount) from fnd_cheque_upload_line l where l.batch_id = b.batch_id) amount
  from fnd_cheque_upload_batch b
 where b.batch_id = :ID

-- name: BATCH_LINES
select line_no, line_status, error_message, client_id, raw_client_id, party_tax_id, raw_party_tax_id, cheque_no, raw_cheque_no,
       due_date, raw_due_date, cheque_amount, raw_cheque_amount, issuer_bank_code, raw_issuer_bank_code, issuer_bank_name, target_cheque_ref
  from fnd_cheque_upload_line
 where batch_id = :ID
 order by line_no

-- name: CHEQUE
select c.cheque_id, c.cheque_ref, c.cheque_no, c.cheque_date, c.due_date, c.cheque_amount, c.currency_code, c.legal_entity_id,
       c.issuer_bank_code, c.issuer_bank_name, c.issuer_bank_branch, c.issuer_account_no,
       c.drawer_name_en, c.drawer_name_ar, c.drawer_national_id, c.drawer_account_no,
       c.customer_id, c.customer_name_en, c.customer_name_ar, c.party_tax_id, c.source_module, c.source_ref_type, c.source_ref_id,
       c.drawdown_id, c.schedule_id, c.payment_id, c.status_code, c.current_location_code, c.current_custodian_user,
       c.deposit_bank_account_id, c.deposit_batch_id, c.received_dt, c.deposit_dt, c.collection_dt, c.rejection_dt, c.withdrawal_dt,
       c.rejection_reason_code, c.rejection_notes, c.is_replaced, c.replacement_cheque_id, c.gl_posted, c.gl_journal_ref, c.gl_post_dt,
       c.notes, c.created_dt, c.created_by, c.updated_dt, c.updated_by, c.batch_id,
       (select max(legal_entity_code) from fin_legal_entities where legal_entity_id = c.legal_entity_id) legal_entity_name,
       (select max(name_en) from lkp_cheque_status where status_code = c.status_code) status_name,
       (select bool_or(is_terminal) from lkp_cheque_status where status_code = c.status_code) is_terminal,
       (select max(location_name) from lkp_cheque_location where location_code = c.current_location_code) location_name,
       (select max(account_no) from trs_bank_accounts where account_id = c.deposit_bank_account_id) deposit_account_name,
       (select max(batch_ref) from fnd_cheque_deposit_batch where deposit_batch_id = c.deposit_batch_id) deposit_batch_ref,
       (select max(name_en) from lkp_cheque_rejection_reason where reason_code = c.rejection_reason_code) reason_name,
       (select max(batch_no) from fnd_cheque_upload_batch where batch_id = c.batch_id) batch_no
  from fnd_cheque c
 where c.cheque_id = :ID

-- name: CHEQUE_EVENTS
select e.event_id, e.event_type, t.event_desc_en, e.event_dt, e.event_amount, e.currency_code, e.from_location_code, e.to_location_code,
       fl.location_name from_name, tl.location_name to_name, e.old_status_code, e.new_status_code, os.name_en old_name, ns.name_en new_name,
       e.handover_by, e.received_by, e.created_by, e.bank_ref, rr.name_en reason, e.notes, e.movement_group_id, g.movement_ref
  from fnd_cheque_event e
  left join lkp_cheque_event_type t on t.event_type = e.event_type
  left join lkp_cheque_location fl on fl.location_code = e.from_location_code
  left join lkp_cheque_location tl on tl.location_code = e.to_location_code
  left join lkp_cheque_status os on os.status_code = e.old_status_code
  left join lkp_cheque_status ns on ns.status_code = e.new_status_code
  left join lkp_cheque_rejection_reason rr on rr.reason_code = e.rejection_reason_code
  left join fnd_cheque_event_group g on g.movement_group_id = e.movement_group_id
 where e.cheque_id = :ID
 order by e.event_dt desc, e.event_id desc

-- name: LIST_CURRENCIES
select currency_code k, currency_name_en || ' (' || currency_code || ')' v from fin_currencies order by currency_name_en

-- name: LIST_ENTITIES
select to_char(legal_entity_id) k, legal_entity_code v from fin_legal_entities order by legal_entity_code

-- name: LIST_BANKS
select bank_code k, bank_code || ' · ' || bank_name_en v from lkp_cheque_issuer_bank where active_flag = 'Y' order by bank_code

-- name: LIST_REASONS
select reason_code k, name_en v from lkp_cheque_rejection_reason order by name_en

-- name: LIST_ACCOUNTS
select to_char(account_id) k, account_no v from trs_bank_accounts order by account_no fetch first 500 rows only

-- name: ACTIONS
select k.action_code, k.action_name_en, k.action_name_ar, k.event_type, t.event_desc_en, k.target_location_code,
       k.allowed_from_status, k.allowed_from_location, k.require_deposit_batch, k.require_bank_ref,
       k.require_rejection_reason, k.require_notes, k.package_procedure_name, k.display_seq, k.active_flag,
       k.created_by, k.created_dt, k.updated_by, k.updated_dt
  from fnd_cheque_action_catalog k
  left join lkp_cheque_event_type t on t.event_type = k.event_type
 order by k.display_seq nulls last, k.action_name_en

-- name: MOVES
select g.movement_group_id, g.event_type, g.event_dt, g.effective_dt, g.deposit_batch_id, g.to_location_code, g.movement_ref,
       g.notes, g.created_by, g.created_dt, t.event_desc_en, l.location_name, d.batch_ref,
       (select count(*) from fnd_cheque_event e where e.movement_group_id = g.movement_group_id) cheques,
       (select sum(nvl(e.event_amount, c.cheque_amount)) from fnd_cheque_event e join fnd_cheque c on c.cheque_id = e.cheque_id
         where e.movement_group_id = g.movement_group_id) amount,
       (select max(nvl(e.currency_code, c.currency_code)) from fnd_cheque_event e join fnd_cheque c on c.cheque_id = e.cheque_id
         where e.movement_group_id = g.movement_group_id) currency
  from fnd_cheque_event_group g
  left join lkp_cheque_event_type t on t.event_type = g.event_type
  left join lkp_cheque_location l on l.location_code = g.to_location_code
  left join fnd_cheque_deposit_batch d on d.deposit_batch_id = g.deposit_batch_id
 order by g.event_dt desc, g.movement_group_id desc
 fetch first 1001 rows only

-- name: MV_CHEQUES
select c.cheque_id, c.cheque_no, c.cheque_ref, c.customer_id, c.customer_name_en, c.issuer_bank_name, c.issuer_bank_code,
       c.cheque_amount, c.currency_code, c.current_location_code, loc.location_name, c.current_custodian_user,
       c.last_event_dt, c.status_code, s.name_en status_name, c.due_date
  from fnd_cheque c
  left join lkp_cheque_location loc on loc.location_code = c.current_location_code
  left join lkp_cheque_status s on s.status_code = c.status_code
 where c.status_code not in ('SETTLED', 'CANCELLED', 'REPLACED')
 order by c.last_event_dt desc nulls last, c.cheque_id desc

-- name: MV_ACTIONS
select k.action_code, k.action_name_en, k.event_type, t.event_desc_en, k.target_location_code, l.location_name,
       k.allowed_from_status, k.allowed_from_location, k.active_flag
  from fnd_cheque_action_catalog k
  left join lkp_cheque_event_type t on t.event_type = k.event_type
  left join lkp_cheque_location l on l.location_code = k.target_location_code
 order by k.action_name_en

-- name: MV_BANKS
select to_char(bank_id) a, bank_code || ' -- ' || bank_name_en b from trs_banks order by bank_code

-- name: MV_ACCOUNTS
select to_char(account_id) a, account_no || ' - (' || currency_code || ') - ' || account_type_code b, to_char(bank_id) c
  from trs_bank_accounts order by account_no

-- name: MOVE
select g.movement_group_id, g.event_type, g.to_location_code, g.movement_ref, g.notes, g.event_dt, g.effective_dt, g.deposit_batch_id,
       g.created_by, g.created_dt,
       (select max(event_desc_en) from lkp_cheque_event_type where event_type = g.event_type) type_name,
       (select max(location_name) from lkp_cheque_location where location_code = g.to_location_code) location_name,
       (select max(batch_ref) from fnd_cheque_deposit_batch where deposit_batch_id = g.deposit_batch_id) deposit_ref
  from fnd_cheque_event_group g
 where g.movement_group_id = :ID

-- name: MOVE_LINES
select e.event_id, e.cheque_id, c.cheque_ref, c.cheque_no, c.customer_id, c.customer_name_en, c.issuer_bank_code, c.due_date,
       nvl(e.event_amount, c.cheque_amount) amount, nvl(e.currency_code, c.currency_code) currency,
       e.from_location_code, e.to_location_code, fl.location_name from_name, tl.location_name to_name,
       e.old_status_code, e.new_status_code, os.name_en old_name, ns.name_en new_name,
       e.handover_by, e.received_by, e.received_dt, e.bank_ref, rr.name_en reason
  from fnd_cheque_event e
  join fnd_cheque c on c.cheque_id = e.cheque_id
  left join lkp_cheque_location fl on fl.location_code = e.from_location_code
  left join lkp_cheque_location tl on tl.location_code = e.to_location_code
  left join lkp_cheque_status os on os.status_code = e.old_status_code
  left join lkp_cheque_status ns on ns.status_code = e.new_status_code
  left join lkp_cheque_rejection_reason rr on rr.reason_code = e.rejection_reason_code
 where e.movement_group_id = :ID
 order by c.due_date, c.cheque_ref

-- name: HOME_COUNTS
select (select count(*) from fnd_cheque) chq,
       (select sum(cheque_amount) from fnd_cheque) amt,
       (select count(*) from fnd_cheque where due_date between trunc(sysdate) and trunc(sysdate) + 30) due,
       (select sum(cheque_amount) from fnd_cheque where due_date between trunc(sysdate) and trunc(sysdate) + 30) due_amt,
       (select count(*) from fnd_cheque where created_dt >= trunc(sysdate, 'MM')) month_cnt,
       (select count(*) from fnd_cheque_upload_batch) b_all,
       (select count(*) from fnd_cheque_upload_batch where batch_status = 'SUBMITTED') b_sub,
       (select count(*) from fnd_cheque_upload_batch where batch_status = 'HAS_ERRORS') b_err,
       (select count(*) from fnd_cheque_upload_batch where batch_status = 'VALIDATED') b_ok,
       (select count(*) from fnd_cheque_upload_batch where batch_status <> 'SUBMITTED') b_open
  from dual

-- name: HOME_LOCATIONS
select l.location_code, l.location_name, count(c.cheque_id) n, sum(c.cheque_amount) a
  from lkp_cheque_location l left join fnd_cheque c on c.current_location_code = l.location_code
 where l.active_flag = 'Y' group by l.location_code, l.location_name, l.display_seq order by l.display_seq, l.location_name

-- name: HOME_STATUSES
select c.status_code, max(s.name_en) name_en, count(*) n, sum(c.cheque_amount) a
  from fnd_cheque c left join lkp_cheque_status s on s.status_code = c.status_code
 group by c.status_code order by count(*) desc fetch first 10 rows only
