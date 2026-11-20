apex.jQuery(function(){
(function($){
'use strict';
var IC={send:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m22 2-7 20-4-9-9-4z"/><path d="M22 2 11 13"/></svg>',check:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 6 9 17l-5-5"/></svg>',post:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20V3H6.5A2.5 2.5 0 0 0 4 5.5z"/><path d="M4 19.5A2.5 2.5 0 0 0 6.5 22H20v-5"/><path d="m9 10 2 2 4-4"/></svg>',ban:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="9"/><path d="m5.7 5.7 12.6 12.6"/></svg>',search:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="11" cy="11" r="7"/><path d="m21 21-4.3-4.3"/></svg>'};
var KPI=[['DRAFT','','Drafts','Action: Submit for approval','#9AAAB2'],['SUBMITTED','','Inbox: waiting approval','Action: Approve or Reject','#E87722'],
  ['APPROVED','','Inbox: ready to post','Action: Post to ledger','#00636D'],['POSTED','','Posted','Final, no action','#14734F'],
  ['ERROR','!','Errors & rejected','Action: fix, then submit or post again','#B42318'],['','','All journals','Search only','#5F7B8A']];
var SEGKEY={COMPANY:'company',BRANCH:'branch',COST_CENTER:'cost_center',NATURAL:'account',BUSINESS_LINE:'business_line',FUTURE1:'future1',FUTURE2:'future2'};
var S={d:null,rows:[],sel:{},stage:'',onOk:null,init:false};
function esc(v){return apex.util.escapeHTML(v==null?'':String(v));}
function lbl(v){v=String(v||'').replace(/_/g,' ').toLowerCase();return v.replace(/\b\w/g,function(c){return c.toUpperCase();});}
function money(n){return (+n||0).toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2});}
function fd(d){if(!d)return '-';var p=String(d).split('-');var m=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];return p.length<3?d:p[2]+' '+m[+p[1]-1]+' '+p[0];}
function clean(m){return String(m||'').replace(/^ORA-\d+:\s*/,'').replace(/\nORA-[\s\S]*$/,'');}

var GN_ICO={ok:'<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6 9 17l-5-5"/></svg>',
  err:'<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round"><path d="M12 7v6M12 17h.01"/></svg>',
  warn:'<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round"><path d="M12 7v6M12 17h.01"/></svg>',
  info:'<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round"><path d="M12 11v6M12 7h.01"/></svg>'};
function notify(kind,title,msg,ms){var w=document.getElementById('gnWrap');if(!w){w=document.createElement('div');w.id='gnWrap';w.className='gn-wrap';w.setAttribute('aria-live','polite');document.body.appendChild(w);}
  if(apex.message&& apex.message.clearErrors)apex.message.clearErrors();
  ms=ms||(kind==='ok'?4500:(kind==='err'?12000:7000));
  var n=document.createElement('div');n.className='gn '+kind;n.setAttribute('role',kind==='err'?'alert':'status');n.style.setProperty('--d',ms+'ms');
  n.innerHTML='<span class="gn-i">'+GN_ICO[kind]+'</span><div class="gn-b"><b>'+esc(title)+'</b>'+(msg?'<p>'+esc(msg)+'</p>':'')+'</div><button type="button" class="gn-x" aria-label="Close">&times;</button><i class="gn-t"></i>';
  function close(){if(!n.parentNode)return;n.classList.add('out');setTimeout(function(){if(n.parentNode)n.parentNode.removeChild(n);},230);}
  n.querySelector('.gn-x').addEventListener('click',close);var t=setTimeout(close,ms);
  n.addEventListener('mouseenter',function(){clearTimeout(t);n.querySelector('.gn-t').style.animationPlayState='paused';});
  n.addEventListener('mouseleave',function(){t=setTimeout(close,2500);});
  while(w.children.length>=3)w.removeChild(w.firstChild);w.appendChild(n);}
function showErr(m,title){m=clean(m);if($('#gpMb').hasClass('open')){$('#gpErr').text(m).addClass('show');$('#gpMok').prop('disabled',false);}else{notify('err',title||'Could not complete',m);}}
function call(name,data,ok){apex.server.process(name,data,{dataType:'json'}).then(function(d){if(d&& d.ok===false){showErr(d.message||'The request failed.');return;}ok(d||{});},function(x){showErr((x&& x.responseText)||'The request failed.');});}
var STAGE_LBL={DRAFT:'Draft',SUBMITTED:'Waiting approval',APPROVED:'Ready to post',POSTED:'Posted',ERROR:'Error',REJECTED:'Rejected',CANCELLED:'Cancelled',REVERSED:'Reversed'};
function filters(){var f={stage:S.stage};$('#gsF [data-f]').each(function(){var v=$.trim($(this).val());if(v)f[$(this).data('f')]=v;});
  var sg={};$('#gsF [data-seg]').each(function(){var v=$.trim($(this).val()).toUpperCase();if(v)sg[$(this).data('seg')]=v;});f.segments=sg;return f;}
function search(){S.sel={};call('J51_SEARCH',{x01:JSON.stringify(filters()),x02:S.init?'':'init'},function(d){
  if(!S.init){S.d=d;S.init=true;$('#gsNew').attr('href',d.newUrl||'#');$('#gsLedger').attr('href',d.ledgerUrl||'#');if(renderFilters()){search();return;}}
  S.rows=d.rows||[];S.kpi=d.kpis||{};renderKpis();renderRows(d.more);try{sessionStorage.setItem('gsF',JSON.stringify(filters()));}catch(e){}});}
function renderKpis(){$('#gsKpis').html(KPI.map(function(k){var n=k[0]===''?S.kpi.ALL:(k[0]==='ERROR'?(+S.kpi.ERROR||0)+(+S.kpi.REJECTED||0):S.kpi[k[0]]);
  return '<button type="button" class="gs-k'+(S.stage===k[0]?' on':'')+'" data-stage="'+k[0]+'" style="--c:'+k[4]+'" aria-pressed="'+(S.stage===k[0])+'">'
    +'<span class="gs-kh"><i></i>'+k[2]+'</span><b>'+(n||0)+'</b><small>'+k[3]+'</small></button>';}).join(''));}
function opt(v,t){return '<option value="'+esc(v)+'">'+esc(t)+'</option>';}
function renderFilters(){var d=S.d;
  var segs=(d.segments||[]).filter(function(s){var n=((d.values||{})[s.code]||[]).filter(function(v){return v[2]==='Y';}).length;return !(s.defMode==='FIXED'&& s.def)&&n!==1;});
  var h='<div class="gs-g gs-g3">'
  +'<div class="span3"><label for="gsQ">Search</label><input type="search" id="gsQ" data-f="q" placeholder="Journal no, name, TRAN_SET_ID (module transaction set) or reference"></div>'
  +'<div class="span2"><label for="gsPer">Period</label><select id="gsPer" data-f="period">'+opt('','All periods')+(d.periods||[]).map(function(p){return opt(p,p+(p===d.currentPeriod?'  (current)':''));}).join('')+'</select></div>'
  +'<div><label for="gsSrc" title="Where the journal comes from: entered by hand (MANUAL) or sent by a module such as Mortgage or Leasing">Source</label><select id="gsSrc" data-f="source">'+opt('','All sources')+(d.sources||[]).map(function(s){return opt(s.code,s.name);}).join('')+'</select></div>'
  +((d.ledgers||[]).length>1?'<div><label for="gsLed">Ledger</label><select id="gsLed" data-f="ledger">'+opt('','All ledgers')+(d.ledgers||[]).map(function(l){return opt(l.id,l.code);}).join('')+'</select></div>':'')
  +'</div>';
  if(segs.length)h+='<div class="gs-more" id="gsMore"><p class="gp-note" style="margin:0 0 8px">Find journals with a line on these account segments (empty = any):</p><div class="gs-segs" style="border:0;padding:0;margin:0">'
    +segs.map(function(s){return '<div><label for="gsSeg'+s.num+'" title="'+esc(s.name)+'">'+esc(s.name)+(s.code==='NATURAL'?' (starts with)':'')+'</label><input id="gsSeg'+s.num+'" data-seg="'+esc(s.code)+'" list="gsDl'+s.num+'" placeholder="any">'
    +'<datalist id="gsDl'+s.num+'">'+((d.values||{})[s.code]||[]).map(function(v){return '<option value="'+esc(v[0])+'" label="'+esc(v[1])+'"></option>';}).join('')+'</datalist></div>';}).join('')+'</div></div>';
  h+='<div class="gs-fb">'+(segs.length?'<button type="button" class="gf-open" id="gsMoreBtn" aria-expanded="false">Filter by account</button>':'<span></span>')+'<div class="gp-act">'
    +'<button type="button" class="gp-btn gp-btn--sm" id="gsReset">Clear</button><button type="button" class="gp-btn gp-btn--sm gp-btn--pri" id="gsGo">'+IC.search+'<span>Search</span></button></div></div>';
  $('#gsF').html(h);
  var again=false,f=null;
  try{f=JSON.parse(sessionStorage.getItem('gsF')||'null');}catch(e){}
  if(f){$('#gsF [data-f]').each(function(){var k=$(this).data('f');if(f[k]){$(this).val(f[k]);again=true;}});
    var segOn=false;$('#gsF [data-seg]').each(function(){var k=$(this).data('seg');if(f.segments&& f.segments[k]){$(this).val(f.segments[k]);again=true;segOn=true;}});if(segOn)toggleMore(true);}
  else if(d.currentPeriod){$('#gsPer').val(d.currentPeriod);again=true;}
  return again;
}
function toggleMore(on){$('#gsMore').toggleClass('show',on);$('#gsMoreBtn').attr('aria-expanded',on).text(on?'Hide account filter':'Filter by account');}
$(document).on('click','#gsMoreBtn',function(){toggleMore(!$('#gsMore').hasClass('show'));});
$(document).on('click','#gsAllPer',function(e){e.preventDefault();$('#gsPer').val('');search();});
function renderRows(more){var r=S.rows,per=$('#gsPer').val();
  $('#gsCount').html((r.length?(r.length+(more?'+':'')+' journal'+(r.length===1?'':'s')+(more?' (first '+r.length+' shown, narrow the search)':'')):'No journal')
    +(per?' in period '+esc(per)+' &middot; <a href="#" id="gsAllPer" style="text-transform:none;letter-spacing:0;color:#00636D">show all periods</a>':''));
  if(!r.length){$('#gsRows').html('<tr><td colspan="10"><div class="gp-empty"><b>No journal found</b>Change the filters or the status above.</div></td></tr>');updBulk();return;}
  $('#gsRows').html(r.map(function(j){return '<tr data-id="'+j.id+'"'+(S.sel[j.id]?' class="sel"':'')+'><td class="ck"><input type="checkbox" data-ck="'+j.id+'"'+(S.sel[j.id]?' checked':'')+' aria-label="Select '+esc(j.no)+'"></td>'
    +'<td><span class="jn">'+esc(j.no)+'</span></td>'
    +'<td><div class="nm" title="'+esc(j.name)+'">'+esc(j.name)+'</div>'+(j.description?'<small title="'+esc(j.description)+'">'+esc(j.description)+'</small>':'')+'</td>'
    +'<td><span style="white-space:nowrap">'+esc(j.batchNo)+'</span>'+(j.tran?'<small>'+esc(j.tran)+'</small>':'')+'</td>'
    +'<td>'+esc(j.source)+'<small>'+esc(j.category||'')+'</small></td>'
    +'<td style="white-space:nowrap">'+fd(j.date)+'</td><td style="white-space:nowrap">'+esc(j.period||'')+'</td>'
    +'<td class="gj-money">'+money(j.dr)+'<small style="text-align:right">'+esc(j.currency)+' · '+esc(j.lines)+' lines</small></td>'
    +'<td><span class="gj-st '+esc(j.stage)+'">'+esc(STAGE_LBL[j.stage]||lbl(j.stage))+'</span>'+(j.postError?'<small style="color:#B42318;max-width:220px;white-space:normal" title="'+esc(j.postError)+'">Last posting failed: '+esc(j.postError)+'</small>':'')+'</td>'
    +'<td style="white-space:nowrap">'+esc(j.createdBy||'')+'</td></tr>';}).join(''));
  updBulk();}
function selected(){return S.rows.filter(function(j){return S.sel[j.id];});}
function updBulk(){var s=selected(),c=function(st){return s.filter(function(j){return st.indexOf(j.stage)>=0;}).length;};
  $('#gsAll').prop('checked',S.rows.length>0&& s.length===S.rows.length);
  if(!s.length){$('#gsBulk').removeClass('show').html('');return;}
  var b='<b>'+s.length+' selected</b>';
  var n;
  // one step = one action: the buttons depend on the step tab that is open
  var st=S.stage;
  if(st===''){b+='<span style="font-size:12.5px;color:rgba(255,255,255,.75)">To act on journals, open Drafts, Inbox: waiting approval or Inbox: ready to post above.</span>';}
  else if(st==='POSTED'){b+='<span style="font-size:12.5px;color:rgba(255,255,255,.75)">Posted journals are final.</span>';}
  else{
    if((st==='DRAFT'||st==='ERROR')&&(n=c(['DRAFT','REJECTED'])))b+='<button type="button" class="gp-btn gp-btn--sm gp-btn--go" data-bulk="SUBMIT">'+IC.send+'<span>Submit for approval ('+n+')</span></button>';
    if(st==='SUBMITTED'&&(n=c(['SUBMITTED']))){b+='<button type="button" class="gp-btn gp-btn--sm gp-btn--go" data-bulk="APPROVE">'+IC.check+'<span>Approve ('+n+')</span></button>';
      b+='<button type="button" class="gp-btn gp-btn--sm gp-btn--danger" data-bulk="REJECT">'+IC.ban+'<span>Reject ('+n+')</span></button>';}
    if((st==='APPROVED'||st==='ERROR')&&(n=c(['APPROVED','ERROR'])))b+='<button type="button" class="gp-btn gp-btn--sm gp-btn--go" data-bulk="POST" title="Last step: add the amounts to the account balances">'+IC.post+'<span>Post to ledger ('+n+')</span></button>';
    if(st!=='SUBMITTED'&&(n=c(['DRAFT','APPROVED','ERROR','REJECTED'])))b+='<button type="button" class="gp-btn gp-btn--sm gp-btn--danger" data-bulk="CANCEL">'+IC.ban+'<span>Cancel journal ('+n+')</span></button>';
  }
  b+='<button type="button" class="gp-btn gp-btn--sm" id="gsClr" style="margin-left:auto">Clear selection</button>';
  $('#gsBulk').addClass('show').html(b);}
function openM(title,html,okText,onOk,danger){$('#gpMt').text(title);$('#gpMc').html(html);$('#gpErr').removeClass('show').text('');
  $('#gpMok').text(okText).show().prop('disabled',false).toggleClass('gp-btn--danger',!!danger).toggleClass('gp-btn--pri',!danger);S.onOk=onOk;$('#gpMb').addClass('open');}
function closeM(){$('#gpMb').removeClass('open');S.onOk=null;}
$('#gpMx,#gpMcancel').on('click',closeM);
$('#gpMb').on('click',function(e){if(e.target===this)closeM();});
$(document).on('keydown',function(e){if(e.key==='Escape'&&$('#gpMb').hasClass('open'))closeM();});
$('#gpMok').on('click',function(){if(S.onOk){$('#gpMok').prop('disabled',true);S.onOk();}});
var ELIG={SUBMIT:['DRAFT','REJECTED'],APPROVE:['SUBMITTED'],REJECT:['SUBMITTED'],POST:['APPROVED','ERROR'],CANCEL:['DRAFT','APPROVED','ERROR','REJECTED']};
var VERB={SUBMIT:'Submit for approval',APPROVE:'Approve',REJECT:'Reject',POST:'Post to ledger',CANCEL:'Cancel journal'};
var WHAT={SUBMIT:['Send for approval','The journals are sent to another user for approval. Their lines can no longer be changed.',''],
  APPROVE:['Approve','You confirm the journals are correct. They move to "Inbox: ready to post". The account balances do not change yet: posting is the next, separate step.',''],
  REJECT:['Reject','The journals go back to their author as drafts, with your reason, to be corrected and submitted again.',''],
  POST:['Post to the ledger','This is the last step. The amounts of the journals are added to the account balances of their period, so they appear in balances and reports. A posted journal can no longer be changed; it can only be corrected by a reversing journal.','ok'],
  CANCEL:['Cancel','The journals will never be posted. Enter the reason; it is kept in their history.','']};
$(document).on('click','[data-bulk]',function(){var a=$(this).data('bulk'),js=selected().filter(function(j){return ELIG[a].indexOf(j.stage)>=0;});
  var need=(a==='CANCEL'||a==='REJECT');
  var w=WHAT[a],tot=js.reduce(function(t,j){return t+(+j.dr||0);},0);
  var h='<div class="gp-ready '+w[2]+'"><b>What happens:</b> '+esc(w[1])+'</div>'
    +'<div class="gp-tw" style="max-height:240px;border:1px solid #DFE8EC;border-radius:12px"><table class="gp-t"><thead><tr><th>Journal</th><th>Name</th><th>Period</th><th style="text-align:right">Amount</th></tr></thead><tbody>'
    +js.map(function(j){return '<tr><td><code>'+esc(j.no)+'</code></td><td>'+esc(j.name)+'</td><td>'+esc(j.period||'')+'</td><td class="gj-money">'+money(j.dr)+' '+esc(j.currency||'')+'</td></tr>';}).join('')
    +'</tbody></table></div>'
    +(js.length>1?'<p class="gp-note">Total '+money(tot)+'. Each journal is handled on its own: if one fails, it is left unchanged and the others still go through.</p>':'')
    +(need?'<div class="gp-f"><label for="gsNote">Reason (required)</label><textarea id="gsNote"></textarea></div>':(a==='APPROVE'?'<div class="gp-f"><label for="gsNote">Note (optional)</label><textarea id="gsNote"></textarea></div>':''));
  openM(w[0]+' - '+js.length+' journal'+(js.length===1?'':'s'),h,VERB[a]+(js.length>1?' ('+js.length+')':''),function(){var note=$('#gsNote').val()||'';if(need&&!$.trim(note)){showErr('Enter the reason.');return;}
    call('J51_ACTION',{x01:a,x02:js.map(function(j){return j.id;}).join(','),x03:note},function(d){var res=d.results||[],okN=res.filter(function(r){return r.ok;}).length;
      $('#gpMt').text(VERB[a]+': '+okN+' of '+res.length+' done');
      $('#gpMc').html('<div class="gs-res">'+res.map(function(r){return '<div class="'+(r.ok?(r.message?'':'yes'):'no')+'"><code>'+esc(r.no)+'</code><span>'+esc(r.ok?(r.message||'Done'):r.message)+'</span></div>';}).join('')+'</div>');
      $('#gpMok').hide();search();});},a==='CANCEL'||a==='REJECT');});
$(document).on('click','#gsClr',function(){S.sel={};renderRows();});
$(document).on('change','[data-ck]',function(){var id=$(this).data('ck');if(this.checked)S.sel[id]=1;else delete S.sel[id];$(this).closest('tr').toggleClass('sel',this.checked);updBulk();});
$(document).on('change','#gsAll',function(){var on=this.checked;S.sel={};if(on)S.rows.forEach(function(j){S.sel[j.id]=1;});renderRows();});
$(document).on('click','#gsRows tr[data-id] td:not(.ck)',function(){var id=$(this).closest('tr').data('id'),j=S.rows.find(function(x){return String(x.id)===String(id);});if(j&& j.url)apex.navigation.redirect(j.url);});
$(document).on('click','.gs-k',function(){S.stage=String($(this).data('stage')||'');search();});
$(document).on('click','#gsGo',search);
$(document).on('click','#gsReset',function(){$('#gsF [data-f],#gsF [data-seg]').val('');S.stage='';try{sessionStorage.removeItem('gsF');}catch(e){}search();});
$(document).on('keydown','#gsF input',function(e){if(e.key==='Enter'){e.preventDefault();search();}});
$(document).on('change','#gsF select',search);
$(function(){try{var f=JSON.parse(sessionStorage.getItem('gsF')||'null');if(f&& f.stage)S.stage=f.stage;}catch(e){}
  var h=String(location.hash||'').replace('#','');if(/^(DRAFT|SUBMITTED|APPROVED|POSTED|ERROR)$/.test(h)){S.stage=h;try{history.replaceState(null,'',location.href.split('#')[0]);}catch(e){}}
  search();});
})(apex.jQuery);

(function(){
  function open(e){if(e)e.preventDefault();var m=document.getElementById('gfMb');if(!m)return;m.classList.add('open');var x=document.getElementById('gfX');if(x)x.focus();}
  function close(){var m=document.getElementById('gfMb');if(m)m.classList.remove('open');}
  document.addEventListener('click',function(e){var t=e.target.closest?e.target.closest('[data-flow]'):null;if(t){open(e);return;}
    if(e.target.id==='gfX'||e.target.id==='gfMb')close();});
  document.addEventListener('keydown',function(e){if(e.key==='Escape')close();});
})();
});
