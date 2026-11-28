-- Page 40, Before Header "Load Document Metrics".
select count(*) P40_TOTAL, count(distinct application_id) P40_APPS,
       sum(case when document_type_code in ('NATIONAL_ID','UTILITY_BILL','INCOME_CERT') then 1 else 0 end) P40_ID,
       sum(case when document_type_code in ('PROPERTY_OWNERSHIP','REAL_ESTATE_DISPOSAL_CERT','ALLOCATION_DECISION','LAND_HANDOVER','BUILDING_LICENSE','PROPERTY_TAX_CERT') then 1 else 0 end) P40_PROP
  from mff_vw_missing_documents
