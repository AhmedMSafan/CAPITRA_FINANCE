-- Page 24, Before Header "Load Property": the application and its selected property.
select nvl(x.application_no,'Not Found') P24_APP_NO, x.status_code P24_APP_STATUS, x.full_name_ar P24_APPLICANT,
       y.property_id P24_PROPERTY_ID, y.project_id P24_PROJECT_ID, y.governorate_code P24_GOVERNORATE_CODE, y.city_code P24_CITY_CODE,
       y.district_ar P24_DISTRICT_AR, y.neighborhood_ar P24_NEIGHBORHOOD_AR, y.building_no P24_BUILDING_NO, y.floor_no P24_FLOOR_NO,
       y.unit_no P24_UNIT_NO, y.area_sqm P24_AREA_SQM, y.address_ar P24_ADDRESS_AR, y.registration_status_code P24_REGISTRATION_STATUS_CODE,
       y.registry_reference P24_REGISTRY_REFERENCE, y.sale_price P24_SALE_PRICE, y.appraised_value P24_APPRAISED_VALUE, y.maintenance_deposit P24_MAINTENANCE_DEPOSIT
  from (select 1 one from dual) d
  left join (select application_no, status_code, p.full_name_ar from mff_applications a join mff_persons p on p.person_id=a.primary_person_id
              where a.application_id=:P24_APPLICATION_ID) x on 1=1
  left join (select p.property_id, p.project_id, p.governorate_code, p.city_code, p.district_ar, p.neighborhood_ar, p.building_no, p.floor_no, p.unit_no,
                    p.area_sqm, p.address_ar, p.registration_status_code, p.registry_reference, p.sale_price, p.appraised_value, p.maintenance_deposit
               from mff_application_property ap join mff_properties p on p.property_id=ap.property_id
              where ap.application_id=:P24_APPLICATION_ID and ap.selected_flag='Y'
              order by ap.app_property_id fetch first 1 rows only) y on 1=1
