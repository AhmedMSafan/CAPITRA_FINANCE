(function(){
window.FINOVA_MFF=window.FINOVA_MFF||{};
var nav={
'dashboard':['Dashboard','لوحة التحكم'],
'fund-intake':['Fund Intake','الوارد من الصندوق'],
'fund-import-batches':['Import Batches','ملفات الاستيراد'],
'fund-staging':['Staging Cases','الحالات الواردة'],
'fund-upload':['Upload Fund File','رفع ملف الصندوق'],
'fund-process':['Process Batch','معالجة الدفعة'],
'origination':['Origination','طلبات التمويل'],
'applications':['Applications','طلبات التمويل'],
'credit-inquiry':['Credit Inquiry','الاستعلام الائتماني'],
'document-review':['Document Review','مراجعة المستندات'],
'work-queue':['My Work Queue','مهامي'],
'financing':['Financing','التمويل'],
'mortgage-calculator':['Mortgage Calculator','حاسبة التمويل'],
'fund-subsidy':['Fund Subsidy','دعم الصندوق'],
'finance-offer':['Finance Offer','عرض التمويل'],
'tripartite-contract':['Tripartite Contract','العقد الثلاثي'],
'disbursement':['Disbursement','صرف التمويل'],
'servicing':['Servicing & Collections','خدمة التمويل والتحصيل'],
'installments-due':['Installments Due','الأقساط المستحقة'],
'delinquency':['Delinquency','التعثر'],
'mis-reports':['MIS & Reports','التقارير الإدارية']
};
function parts(s){
 if(!s)return null;
 var a=s.search(/[؀-ۿ]/); if(a<0)return null;
 var sep=s.lastIndexOf(' / ',a); var n=3;
 if(sep<0){sep=s.lastIndexOf('/',a);n=1;} if(sep<0)return null;
 var en=s.substring(0,sep).trim(), ar=s.substring(sep+n).trim();
 return en&&ar?[en,ar]:null;
}
function translateElement(el,l){
 if(!el)return;
 var en=el.getAttribute('data-mff-en'), ar=el.getAttribute('data-mff-ar');
 if(!en||!ar){var p=parts((el.textContent||'').trim());if(!p)return;en=p[0];ar=p[1];el.setAttribute('data-mff-en',en);el.setAttribute('data-mff-ar',ar);}
 // Elements in the selector list are text containers; keep their classes/attributes.
 el.textContent=(l==='AR'?ar:en);
}
function translateNav(l){Object.keys(nav).forEach(function(id){var root=document.getElementById(id);if(!root)return;var lab=root.querySelector('.a-TreeView-label,.t-LinksList-label,.a-Menu-label')||root;if(lab)lab.textContent=nav[id][l==='AR'?1:0];});}
function translatePage(l){
 var sel=['.t-Form-label','.t-Button-label','.t-Region-title','.a-IRR-headerLink','.a-GV-headerLabel','.mff-list-title h1','.mff-list-title p','.mff-list-tag','.mff-action-strip a','.fin-kpi-label','.fin-stage-label','.fin-card-header span','.fin-card-link'];
 document.querySelectorAll(sel.join(',')).forEach(function(el){translateElement(el,l);});
}
FINOVA_MFF.applyLanguage=function(){
 var l=localStorage.getItem('FINOVA_MFF_LANG')||'EN';
 document.documentElement.lang=(l==='AR'?'ar':'en');document.documentElement.dir=(l==='AR'?'rtl':'ltr');
 if(document.body){document.body.classList.toggle('mff-rtl',l==='AR');translateNav(l);translatePage(l);}
};
FINOVA_MFF.setLanguage=function(l){localStorage.setItem('FINOVA_MFF_LANG',l==='AR'?'AR':'EN');FINOVA_MFF.applyLanguage();location.reload();};
document.addEventListener('DOMContentLoaded',FINOVA_MFF.applyLanguage);
if(window.apex&&apex.jQuery){apex.jQuery(document).on('apexafterrefresh',function(){FINOVA_MFF.applyLanguage();});}
})();
