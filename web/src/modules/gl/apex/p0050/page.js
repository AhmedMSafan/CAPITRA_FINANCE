apex.jQuery(function(){
(function($){
'use strict';
var IC={trash:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 6h18M8 6V4h8v2M6 6l1 14h10l1-14"/></svg>',save:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"/><path d="M17 21v-8H7v8M7 3v5h8"/></svg>',send:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m22 2-7 20-4-9-9-4z"/><path d="M22 2 11 13"/></svg>',check:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 6 9 17l-5-5"/></svg>',post:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20V3H6.5A2.5 2.5 0 0 0 4 5.5z"/><path d="M4 19.5A2.5 2.5 0 0 0 6.5 22H20v-5"/><path d="m9 10 2 2 4-4"/></svg>',ban:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="9"/><path d="m5.7 5.7 12.6 12.6"/></svg>',undo:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M9 14 4 9l5-5"/><path d="M4 9h11a5 5 0 0 1 0 10h-3"/></svg>'};
var SEGKEY={COMPANY:'company',BRANCH:'branch',COST_CENTER:'cost_center',NATURAL:'account',BUSINESS_LINE:'business_line',FUTURE1:'future1',FUTURE2:'future2'};
var SEGORDER=['COMPANY','BRANCH','COST_CENTER','NATURAL','BUSINESS_LINE','FUTURE1','FUTURE2'];
var S={id:null,d:null,j:null,lines:[],dirty:false,lineErr:{},onOk:null,busy:false};
function esc(v){return apex.util.escapeHTML(v==null?'':String(v));}
function lbl(v){v=String(v||'').replace(/_/g,' ').toLowerCase();return v.replace(/\b\w/g,function(c){return c.toUpperCase();});}
function U(v){return String(v==null?'':v).trim().toUpperCase();}
function num(v){if(v==null)return 0;var s=String(v).replace(/,/g,'').trim();if(s==='')return 0;var n=Number(s);return isFinite(n)?n:NaN;}
function r2(n){return Math.round(n*100)/100;}
function money(n){return (isFinite(n)?n:0).toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2});}
function drcr(n){n=+n||0;return money(Math.abs(n))+(n>0?' Dr':(n<0?' Cr':''));}
function fd(d){if(!d)return '-';var p=String(d).split('-');var m=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];return p.length<3?d:p[2]+' '+m[+p[1]-1]+' '+p[0];}
function stage(j){if(!j)return 'NEW';var js=U(j.journalStatus),ap=U(j.approvalStatus),ps=U(j.postingStatus);
  if(js==='CANCELLED')return 'CANCELLED';if(js==='REVERSED'||ps==='REVERSED')return 'REVERSED';if(ps==='POSTED')return 'POSTED';
  if(js==='ERROR'||ps==='POSTING_ERROR')return 'ERROR';if(ap==='SUBMITTED'||ap==='IN_REVIEW')return 'SUBMITTED';
  if(ap==='APPROVED'||ap==='NOT_REQUIRED')return 'APPROVED';if(ap==='REJECTED'||ap==='RETURNED')return 'REJECTED';return 'DRAFT';}
function editable(){return !S.j||!!S.j.canEdit;}
function call(name,data,ok,fail){
  S.busy=true;$('.gj-acts .gp-btn').prop('disabled',true);
  apex.server.process(name,data,{dataType:'json'}).then(function(d){S.busy=false;renderBar();
      if(d&& d.ok===false){if(fail)fail(d);else showErr(d.message||'The request failed.');return;}ok(d||{});},
    function(x){S.busy=false;renderBar();showErr((x&& x.responseText)||'The request failed.');});
}
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
function showErr(m,title){m=clean(m);
  if($('#gpMb').hasClass('open')){$('#gpErr').text(m).addClass('show');$('#gpMok').prop('disabled',false);}else{notify('err',title||'Could not complete',m);}}
function ok(m,title){notify('ok',title||'Done',m);}
function fieldErr(id,m){var f=$('#'+id);f.addClass('bad');var e=f.parent().find('.gj-ferr');if(!e.length){e=$('<div class="gj-ferr"></div>');f.after(e);}e.text(m).addClass('show');f.trigger('focus');}
$(document).on('input change','#gjHead .bad',function(){$(this).removeClass('bad');$(this).parent().find('.gj-ferr').removeClass('show');});
// ------------------------------------------------------------------ reference data
function ledger(){var id=S.j?S.j.ledgerId:($('#gjLedger').val()||S.d.defaultLedger);return (S.d.ledgers||[]).find(function(l){return String(l.id)===String(id);})||S.d.ledgers[0];}
function coa(){var l=ledger();return l&& S.d.coas?S.d.coas[l.coa]:null;}
function segs(){var c=coa();return c?c.segments:[];}
function vals(code){var c=coa();return c&& c.values?(c.values[code]||[]):[];}
function findVal(code,v){v=U(v);var a=vals(code);for(var i=0;i<a.length;i++){if(a[i][0]===v)return a[i];}return null;}
function segOf(code){return segs().find(function(x){return x.code===code;})||{};}
function hidden(code){var a=vals(code),sg=segOf(code);
  if(sg.def&& sg.defMode==='FIXED')return !S.lines.some(function(l){return l.seg[code]&&U(l.seg[code])!==U(sg.def);});
  return (code==='FUTURE1'||code==='FUTURE2')&& a.length===1;}
function periodFor(dateStr){var l=ledger();if(!l||!dateStr)return null;var best=null;
  (S.d.periods||[]).forEach(function(p){if(String(p.ledgerId)===String(l.id)&& p.start<=dateStr&& p.end>=dateStr){if(!best||(best.adj==='Y'&& p.adj!=='Y'))best=p;}});return best;}
// ------------------------------------------------------------------ load
function load(id){
  call('J50_LOAD',{x01:id||''},function(d){
    S.d=d;S.j=d.journal||null;S.id=S.j?S.j.id:null;S.dirty=false;S.lineErr={};
    if(S.j&& d.selfUrl&& window.history&& history.replaceState){try{history.replaceState(null,'',d.selfUrl);}catch(e){}}
    S.lines=(S.j&& S.j.lines&& S.j.lines.length)?S.j.lines.map(function(l){return {seg:l.seg||{},dr:l.dr?r2(+l.dr).toFixed(2):'',cr:l.cr?r2(+l.cr).toFixed(2):'',d:l.description||'',concat:l.concat||''};}):[blankLine(null),blankLine(null)];
    $('#gjToList').attr('href',d.listUrl||'#');$('#gjNewBtn').attr('href',d.newUrl||'#');
    renderAll();
  });
}
function blankLine(prev){var seg={};segs().forEach(function(s){var a=vals(s.code);
    if(s.code==='NATURAL')return;
    if(s.def&& s.defMode==='FIXED')seg[s.code]=s.def;
    else if(prev&& prev.seg[s.code])seg[s.code]=prev.seg[s.code];
    else if(s.def)seg[s.code]=s.def;
    else if(a.length===1)seg[s.code]=a[0][0];});
  return {seg:seg,dr:'',cr:'',d:prev?prev.d:'',concat:''};}
// ------------------------------------------------------------------ render
function renderAll(){renderHero();renderNext();renderMsgs();renderHead();renderLines();renderBar();renderMore();}
function renderHero(){var j=S.j,st=stage(j);
  $('#gjTitle').text(j?(j.journalNo+' · '+j.name):'New journal');
  var h=j?'<span class="gj-st">'+esc(lbl(st==='APPROVED'?'Ready to post':st))+'</span>'+(j.reversedBy?'<span class="gj-st REVERSED">Reversed</span>':'')+'<small>'+esc(j.sourceCode)+' · '+esc(j.categoryCode)+' · batch '+esc(j.batchNo)+(j.tranSetId?' · TRAN_SET_ID '+esc(j.tranSetId):'')+'</small>'
    :'<small>Manual journal · you enter and submit it, another user approves it, then it is posted.</small>';
  $('#gjHeroSt').html(h);
}
function renderFlow(){var st=stage(S.j),j=S.j||{};
  var steps=[['Draft','Lines entered and balanced',j.createdBy?('by '+j.createdBy):''],['Submitted','Sent for approval',j.submittedBy?('by '+j.submittedBy):''],
             ['Approved','Checked by another user',j.approvedBy?('by '+j.approvedBy):''],['Posted','Balances updated',j.postedBy?('by '+j.postedBy):'']];
  var at={NEW:0,DRAFT:0,REJECTED:0,SUBMITTED:1,APPROVED:2,ERROR:2,POSTED:4,REVERSED:4,CANCELLED:-1}[st];
  $('#gjFlow').html(steps.map(function(s,i){var c=at<0?'':(i<at?'done':(i===at?(st==='ERROR'||st==='REJECTED'?'bad':'now'):''));
    if(st==='POSTED'&&i===3)c='done';
    var sub=(i===0&&st==='REJECTED')?'Rejected - correct and submit again':(i===2&&st==='ERROR')?'Posting failed - see the messages':(s[2]||s[1]);
    if(st==='CANCELLED'&&i===0)sub='Cancelled';
    return '<div class="gj-step '+c+'"><i>'+(c==='done'?'&#10003;':(i+1))+'</i><div style="min-width:0"><b>'+s[0]+'</b><span>'+esc(sub)+'</span></div></div>';}).join(''));
}
var GUIDE=[['Draft','Whoever enters the journal','Fill in the date, name and the lines (a field per segment). <b>Save draft</b> keeps your work; you can change it any time.','Lines can still change'],
  ['Submit','The same person','When Debit = Credit, click <b>Submit for approval</b>. The period must be open.','Lines are locked from now on'],
  ['Approve','A different user (the approver)','The approver opens this journal (it is listed in <b>Inbox: waiting approval</b>, on the home page and the Journals page) and clicks <b>Approve</b>, or <b>Reject</b> with a reason.','Two people see every journal'],
  ['Post to ledger','The approver, or anyone later','Click <b>Post to ledger</b> on this page (or from <b>Inbox: ready to post</b>). The amounts are added to the account balances of the period.','A posted journal can only be corrected by a reversing journal']];
function stepDone(i){var j=S.j||{};return [[j.createdBy,j.createdAt],[j.submittedBy,j.submittedAt],[j.approvedBy,j.approvedAt],[j.postedBy,j.postedAt]][i];}
function guideOpen(){try{var v=localStorage.getItem('gjGuide');return v===null?true:v==='1';}catch(e){return true;}}
function renderNext(){var j=S.j,st=stage(j),fe=S.d.fourEyes==='Y',k='',t='',m='',n='';
  var per=j?j.periodCode:'';
  if((st==='DRAFT'||st==='REJECTED')&& j.reversalOf){n='1';k=st==='REJECTED'?'warn':'';t=st==='REJECTED'?'Reversal rejected - back to you':'Reversal draft - not sent yet';
    m='This journal reverses <a href="'+esc(j.reversalOf.url)+'"><b>'+esc(j.reversalOf.no)+'</b></a>: the same lines with debit and credit switched (they cannot be changed). Click <b>Submit for approval</b>; another user approves it, then <b>Post to ledger</b> brings the balances back. Not needed? <b>Cancel journal</b>.';}
  else if(st==='NEW'||st==='DRAFT'){n='1';t=j?'Draft - not sent yet':'Start a new journal';m='Enter the lines until <b>Debit = Credit</b>, then click <b>Submit for approval</b>. <b>Save draft</b> keeps your work without sending it.';}
  else if(st==='REJECTED'){n='1';k='warn';t='Rejected - back to you';m=(j.rejectReason?'Reason: <b>'+esc(j.rejectReason)+'</b>. ':'')+'Correct the lines, then submit again.';}
  else if(st==='SUBMITTED'&&!j.canApprove){n='3';k='warn';t='Waiting for another user to approve';
    m='You submitted this journal, so a second person must approve it. The approver signs in with their own user, opens <b>Journals &rarr; Waiting approval</b> and approves it. Need a change? Click <b>Reject</b> to bring it back to draft.';}
  else if(st==='SUBMITTED'){n='3';t='Your turn to approve';m='Check the lines and totals, then click <b>Approve</b>. If something is wrong, click <b>Reject</b>: it goes back to '+esc(j.submittedBy||'its author')+' with your reason. Posting is the next, separate step.';}
  else if(st==='APPROVED'&& j.lastPostError){n='4';k='bad';t='Approved - the last posting attempt failed';m='Reason: <b>'+esc(String(j.lastPostError).replace(/\.\s*$/,''))+'</b>. Fix the cause (for example open the period on the Accounting periods page), then click <b>Post to ledger</b> again.';}
  else if(st==='APPROVED'){n='4';t='Approved - ready to post';m='Approved by '+esc(j.approvedBy||'')+'. Click <b>Post to ledger</b> to add the amounts to the account balances of period '+esc(per)+'.';}
  else if(st==='ERROR'){n='4';k='bad';t='Posting failed';m='The reasons are shown above. Fix the cause (for example open the period on the Accounting periods page), then click <b>Post to ledger</b> again.';}
  else if(st==='POSTED'&& j.reversedBy){var rb=j.reversedBy,rs=U(rb.postingStatus)==='POSTED';n='&#10003;';k='ok';t=rs?'Posted and reversed':'Posted - reversal in progress';
    m='Reversed by <a href="'+esc(rb.url)+'"><b>'+esc(rb.no)+'</b></a> ('+esc(rs?'posted: the balances are back to what they were before this journal':lbl(rb.approvalStatus==='SUBMITTED'?'waiting approval':(rb.approvalStatus==='APPROVED'?'approved, ready to post':'draft, not submitted yet')))+').';}
  else if(st==='POSTED'&& j.reversalOf){n='&#10003;';k='ok';t='Reversal posted';m='This journal reversed <a href="'+esc(j.reversalOf.url)+'"><b>'+esc(j.reversalOf.no)+'</b></a>. The balances of period '+esc(per)+' no longer include it.';}
  else if(st==='POSTED'){n='&#10003;';k='ok';t='Posted';m='The balances of period '+esc(per)+' include this journal. A posted journal cannot be changed; to correct it, click <b>Reverse journal</b>, then enter a new journal with the right amounts.';}
  else if(st==='CANCELLED'){n='&#10005;';k='bad';t='Cancelled';m='This journal will never be posted.';}
  else if(st==='REVERSED'){n='&#10003;';k='ok';t='Reversed';m='This journal was posted and later reversed.';}
  if(!fe)m=m.replace('You submitted this journal, so a second person must approve it. ','');
  var at={NEW:0,DRAFT:0,REJECTED:0,SUBMITTED:2,APPROVED:3,ERROR:3,POSTED:4,REVERSED:4}[st];if(at===undefined)at=-1;
  var open=guideOpen();
  $('#gjNext').attr('class','gj-next '+k).html('<span class="ni">'+n+'</span><div class="nb"><b class="t">'+t+'</b>'+m
    +'<div class="gj-guide'+(open?' open':'')+'" id="gjGuide">'+GUIDE.map(function(g,i){var done=i<at&&!(i===1&&st==='REJECTED'),d=stepDone(i);
      return '<div class="gj-g'+(i===at?' now':'')+(done?' done':'')+'"><div class="gh"><i>'+(done?'&#10003;':(i+1))+'</i>'+g[0]+(i===at?'<span class="gj-here">you are here</span>':'')+'</div>'
      +(done&&d&&d[0]?'<div class="gj-by">Done by '+esc(d[0])+(d[1]?' &middot; '+esc(d[1]):'')+'</div>':'')+'<span style="color:#5F7B8A;font-size:11.5px">'+g[1]+'</span><br>'+g[2]+'<small>'+g[3]+'</small></div>';}).join('')+'</div></div>'
    +'<div style="display:flex;flex-direction:column;align-items:flex-end;gap:8px;flex:0 0 auto"><button type="button" class="gp-btn gp-btn--sm" id="gjGuideBtn" aria-expanded="'+open+'">'+(open?'Hide the steps':'How it works')+'</button>'
    +'<button type="button" class="gf-open" data-flow style="font-size:12.5px">See the diagram</button></div>');
}
$(document).on('click','#gjGuideBtn',function(){var o=!$('#gjGuide').hasClass('open');try{localStorage.setItem('gjGuide',o?'1':'0');}catch(e){}renderNext();});
function renderMsgs(){var j=S.j,m=[];
  if(j&& j.messages&& j.messages.length)m=j.messages;
  var h='';
  if(m.length)h+='<div class="gj-msgs"><b>'+(stage(j)==='ERROR'?'The journal could not be posted':'Please check')+'</b><ul>'+m.map(function(x){return '<li>'+esc(x)+'</li>';}).join('')+'</ul></div>';

  if(j&&!j.manual&&!j.reversalOf&&stage(j)!=='POSTED')h+='<div class="gj-info">This journal comes from <b>'+esc(j.sourceCode)+'</b>. Its lines are created by that module and cannot be changed here; it can be approved, rejected, posted or cancelled.</div>';
  $('#gjMsgs').html(h);
}
function opt(v,t,sel){return '<option value="'+esc(v)+'"'+(String(v)===String(sel)?' selected':'')+'>'+esc(t)+'</option>';}
function renderHead(){var j=S.j||{},d=S.d,ed=editable(),dis=ed?'':' disabled';
  var led=S.j?S.j.ledgerId:(d.defaultLedger||(d.ledgers[0]&& d.ledgers[0].id));
  var cats=(d.categories||[]).slice();if(j.categoryCode&&!cats.some(function(c){return c.code===j.categoryCode;}))cats.push({code:j.categoryCode,name:j.categoryCode});
  var h='<div><label for="gjLedger">Ledger</label><select id="gjLedger"'+(S.j?' disabled':dis)+'>'+(d.ledgers||[]).map(function(l){return opt(l.id,l.code+' · '+l.currency,led);}).join('')+'</select></div>'
   +'<div><label for="gjDate">Accounting date</label><input type="date" id="gjDate" value="'+esc(j.accountingDate||d.today)+'"'+dis+'><div class="gj-per" id="gjPer"></div></div>'
   +'<div><label for="gjCat">Category</label><select id="gjCat"'+dis+'>'+cats.map(function(c){return opt(c.code,c.name+' ('+c.code+')',j.categoryCode||d.defaultCategory);}).join('')+'</select></div>'
   +'<div><label for="gjJdate">Journal date</label><input type="date" id="gjJdate" value="'+esc(j.journalDate||d.today)+'"'+dis+'></div>'
   +'<div class="span2"><label for="gjName">Journal name</label><input type="text" id="gjName" maxlength="240" value="'+esc(j.name||'')+'" placeholder="e.g. October rent accrual"'+dis+'></div>'
   +'<div><label for="gjRef">Reference</label><input type="text" id="gjRef" maxlength="120" value="'+esc(j.referenceNo||'')+'"'+dis+'></div>'
   +'<div><label>Currency</label><input type="text" value="'+esc(j.currency||(ledger()||{}).currency||'')+'" disabled></div>'
   +'<div class="span4"><label for="gjDesc">Description</label><input type="text" id="gjDesc" maxlength="4000" value="'+esc(j.description||'')+'"'+dis+'></div>';
  $('#gjHead').html(h);showPeriod();
}
function showPeriod(){var p=periodFor($('#gjDate').val()),el=$('#gjPer');
  if(!$('#gjDate').val()){el.attr('class','gj-per bad').text('Enter the accounting date.');return;}
  if(!p){el.attr('class','gj-per bad').text('No period of this ledger covers this date.');return;}
  var okS=(p.status==='OPEN'||p.status==='FUTURE_ENTRY')&& p.manual!=='N';
  el.attr('class','gj-per'+(okS?'':' bad')).html('Period <b>'+esc(p.code)+'</b> · '+esc(lbl(p.status))+(okS?'':(p.manual==='N'?' · manual journals not allowed':' · journals cannot be entered')));
}
function segHead(){var h='<tr><th>#</th>';segs().forEach(function(s){if(!hidden(s.code))h+='<th title="'+esc(s.code)+'">'+esc(s.name)+'</th>';});
  return h+'<th>Description</th><th class="r">Debit</th><th class="r">Credit</th><th></th></tr>';}
function width(s){var n=Math.max(+s.maxLen||6,+s.minLen||2);return s.code==='NATURAL'?120:(s.code==='BUSINESS_LINE'?170:Math.min(Math.max(n*11+22,64),120));}
function renderLines(){var ed=editable(),dis=ed?'':' disabled';
  $('#gjLh').html(segHead());
  var dl='';segs().forEach(function(s){if(hidden(s.code))return;dl+='<datalist id="gjDl'+s.num+'">'+vals(s.code).map(function(v){return '<option value="'+esc(v[0])+'" label="'+esc(v[1])+'"></option>';}).join('')+'</datalist>';});
  var cols=2+segs().filter(function(s){return !hidden(s.code);}).length+3;
  $('#gjLb').html(S.lines.map(function(l,i){
    var r='<tr class="ln" data-i="'+i+'"><td class="no">'+(i+1)+'</td>';
    segs().forEach(function(s){if(hidden(s.code))return;var v=l.seg[s.code]||'';var f=v?findVal(s.code,v):null;
      r+='<td style="width:'+width(s)+'px"><input class="gj-in'+(v?(f?' ok':' bad'):'')+'" data-seg="'+esc(s.code)+'" list="gjDl'+s.num+'" value="'+esc(v)+'" maxlength="'+esc(s.maxLen||30)+'" title="'+esc(f?f[1]:s.name)+'" aria-label="'+esc(s.name)+' line '+(i+1)+'"'+dis+'></td>';});
    r+='<td style="min-width:160px"><input class="gj-in txt" data-f="d" value="'+esc(l.d)+'" maxlength="4000" aria-label="Description line '+(i+1)+'"'+dis+'></td>'
      +'<td style="width:130px"><input class="gj-in amt" data-f="dr" inputmode="decimal" value="'+esc(l.dr)+'" aria-label="Debit line '+(i+1)+'"'+dis+'></td>'
      +'<td style="width:130px"><input class="gj-in amt" data-f="cr" inputmode="decimal" value="'+esc(l.cr)+'" aria-label="Credit line '+(i+1)+'"'+dis+'></td>'
      +'<td style="width:34px">'+(ed?'<button type="button" class="gj-x" data-del="'+i+'" title="Remove line" aria-label="Remove line '+(i+1)+'">'+IC.trash+'</button>':'')+'</td></tr>'
      +'<tr class="sub"><td></td><td colspan="'+(cols-1)+'"><div class="gj-sub" id="gjSub'+i+'">'+subHtml(l,i)+'</div></td></tr>';
    return r;}).join('')+'<tr style="display:none"><td>'+dl+'</td></tr>');
  $('.gj-lf').toggle(ed);
  var fx=segs().filter(function(s){return s.def&& s.defMode==='FIXED'&&hidden(s.code);}).map(function(s){return s.name+' '+s.def;});
  $('#gjLnote').html('Type or pick each segment value. New lines start with the defaults set on the Account segments page'+(fx.length?' and always use: <b>'+esc(fx.join(', '))+'</b>':'')+'.');
}
function subHtml(l,i){var parts=[],miss=[],bad=[],warn=[];
  segs().forEach(function(s){var v=l.seg[s.code];if(!v){miss.push(s.name);parts.push('?');return;}parts.push(U(v));if(!findVal(s.code,v))bad.push(s.name+' '+U(v));});
  var acc=l.seg.NATURAL?findVal('NATURAL',l.seg.NATURAL):null;
  if(acc&&acc[2]){var rq=acc[2];if(rq.indexOf('B')>=0&&/^(000|ALL)?$/.test(U(l.seg.BRANCH)))warn.push('branch required');
    if(rq.indexOf('C')>=0&&/^(000|0000|ALL)?$/.test(U(l.seg.COST_CENTER)))warn.push('cost centre required');
    if(rq.indexOf('L')>=0&&/^(000|ALL)?$/.test(U(l.seg.BUSINESS_LINE)))warn.push('business line required');}
  var dr=num(l.dr),cr=num(l.cr),amt=[];
  if(isNaN(dr)||isNaN(cr))amt.push('amount is not a number');else if(dr<0||cr<0)amt.push('amounts cannot be negative');else if(dr>0&&cr>0)amt.push('enter a debit or a credit, not both');
  var delim=(coa()||{}).delimiter||'-';
  var h='<code>'+esc(parts.join(delim))+'</code>';
  if(acc)h+='<span class="an">'+esc(acc[1])+'</span>';
  if(bad.length)h+='<span class="er">Not a posting value: '+esc(bad.join(', '))+(l.seg.NATURAL&&!acc?' (or the account does not allow manual journals)':'')+'</span>';
  if(amt.length)h+='<span class="er">'+esc(amt.join('; '))+'</span>';
  if(warn.length)h+='<span class="wn">'+esc(warn.join(', '))+'</span>';
  if(S.lineErr[i+1])h+='<span class="er">'+esc(S.lineErr[i+1])+'</span>';
  return h;}
function totals(){var dr=0,cr=0,n=0;S.lines.forEach(function(l){var a=num(l.dr),b=num(l.cr);if(isFinite(a))dr+=a;if(isFinite(b))cr+=b;if(a||b)n++;});return {dr:r2(dr),cr:r2(cr),diff:r2(dr-cr),n:n};}
function renderBar(){if(!S.d)return;var t=totals(),bal=t.diff===0&& t.dr>0,j=S.j,st=stage(j),ed=editable(),b=[];
  var mx=Math.max(t.dr,t.cr,1);
  if(ed){b.push('<button type="button" class="gp-btn gj-2l" data-act="SAVE" title="Keeps your work. Nothing is sent; you can come back and change it.">'+IC.save+'<span><b>Save draft</b><small>finish it later</small></span></button>');
         b.push('<button type="button" class="gp-btn gp-btn--hot gj-2l" data-act="SUBMIT" title="Saves the journal and sends it to the approver. It can no longer be changed.">'+IC.send+'<span><b>Submit for approval</b><small>'+(bal?'it is complete: save and send':'needs Debit = Credit first')+'</small></span></button>');}
  if(!ed&&j&& j.canSubmit)b.push('<button type="button" class="gp-btn gp-btn--hot gj-2l" data-act="SUBMIT" title="Sends the reversal to the approver.">'+IC.send+'<span><b>Submit for approval</b><small>send the reversal</small></span></button>');
  if(j&& j.canReject)b.push('<button type="button" class="gp-btn gp-btn--danger" data-act="REJECT">'+IC.undo+'<span>Reject</span></button>');
  if(j&& j.canApprove)b.push('<button type="button" class="gp-btn gp-btn--hot" data-act="APPROVE">'+IC.check+'<span>Approve</span></button>');
  if(j&& j.canPost)b.push('<button type="button" class="gp-btn gp-btn--hot" data-act="POST" title="Last step: add the amounts to the account balances">'+IC.post+'<span>Post to ledger</span></button>');
  if(j&& j.canCancel)b.push('<button type="button" class="gp-btn gp-btn--danger" data-act="CANCEL">'+IC.ban+'<span>Cancel journal</span></button>');
  if(j&& j.canReverse)b.push('<button type="button" class="gp-btn gp-btn--danger" data-act="REVERSE" title="Create a journal that undoes this one">'+IC.undo+'<span>Reverse journal</span></button>');
  $('#gjBar').html('<div class="gj-tot"><small>Debit</small><b>'+money(t.dr)+'</b></div><div class="gj-tot"><small>Credit</small><b>'+money(t.cr)+'</b></div>'
    +'<div class="gj-tot gj-diff'+(bal?' ok':'')+'"><small>Difference</small><b>'+money(Math.abs(t.diff))+'</b></div>'
    +'<div class="gj-meter" aria-hidden="true"><i style="width:'+(t.dr/mx*50).toFixed(1)+'%"></i><i class="c" style="width:'+(t.cr/mx*50).toFixed(1)+'%"></i></div>'
    +'<span class="gj-bal'+(bal?' ok':'')+'">'+(bal?IC.check+'Balanced':(t.dr+t.cr===0?'No amounts yet':(t.diff>0?'Debit exceeds credit':'Credit exceeds debit')))+'</span>'
    +'<span style="font-size:12px;color:rgba(255,255,255,.65)">'+t.n+' line'+(t.n===1?'':'s')+(S.dirty?' · unsaved changes':'')+'</span>'
    +'<div class="gj-acts">'+b.join('')+'</div>');
  if(S.busy)$('.gj-acts .gp-btn').prop('disabled',true);
}
function renderMore(){var j=S.j;if(!j){$('#gjMore').html('');return;}
  var ev=(j.history||[]).map(function(e){return '<div class="gj-ev"><i></i><div><b>'+esc(lbl(e.action))+'</b>'+(e.status?' <span class="gj-st '+esc(e.status)+'" style="height:18px;font-size:10.5px">'+esc(lbl(e.status))+'</span>':'')
    +(e.notes?'<p>'+esc(e.notes)+'</p>':'')+'</div><small>'+esc(e.by||'')+'<br>'+esc(e.at||'')+'</small></div>';}).join('');
  var kv=[['Journal no',j.journalNo],['Batch',j.batchNo],['TRAN_SET_ID',j.tranSetId],['Source',j.sourceCode],['Period',j.periodCode],['Journal status',lbl(j.journalStatus)],['Approval',lbl(j.approvalStatus)],['Posting',lbl(j.postingStatus)],
          ['Created',(j.createdBy||'')+' · '+(j.createdAt||'')],['Submitted',j.submittedBy?j.submittedBy+' · '+j.submittedAt:''],['Approved',j.approvedBy?j.approvedBy+' · '+j.approvedAt:''],['Posted',j.postedBy?j.postedBy+' · '+j.postedAt:''],
          ['Source document',[j.sourceDocType,j.sourceDocNo||j.sourceDocId].filter(Boolean).join(' ')],['Reversal of',j.reversalOf?j.reversalOf.no:''],['Reversed by',j.reversedBy?j.reversedBy.no:'']];
  var imp=j.impacts||[],ih='';
  if(imp.length){ih='<div class="gj-card" style="grid-column:1/-1"><div class="gp-sec" style="margin:0 0 6px"><h2>Posting result: account balances before and after</h2></div>'
    +'<p class="gp-note" style="margin:0 0 10px">Balance of each account combination in period '+esc(imp[0].period)+', just before and just after this journal was posted (Dr positive, Cr negative).</p>'
    +'<div class="gp-tw"><table class="gp-t"><thead><tr><th>#</th><th>Account combination</th><th style="text-align:right">Debit</th><th style="text-align:right">Credit</th><th style="text-align:right">Balance before</th><th style="text-align:right">Balance after</th><th></th></tr></thead><tbody>'
    +imp.map(function(x){return '<tr><td>'+esc(x.line)+'</td><td><code>'+esc(x.concat)+'</code><span class="ar" style="direction:ltr">'+esc(x.accountName||'')+'</span></td>'
      +'<td class="gj-money">'+(+x.dr?money(+x.dr):'')+'</td><td class="gj-money">'+(+x.cr?money(+x.cr):'')+'</td>'
      +'<td class="gj-money">'+drcr(x.before)+'</td><td class="gj-money"><b>'+drcr(x.after)+'</b></td>'
      +'<td><a class="gf-open" style="font-size:12.5px" href="'+esc((S.d.ledgerUrl||j.ledgerUrl||'#'))+'#account='+encodeURIComponent(x.account||'')+'">Account ledger</a></td></tr>';}).join('')
    +'</tbody></table></div></div>';}
  $('#gjMore').html(ih+'<div class="gj-card"><div class="gp-sec" style="margin:0 0 10px"><h2>Details</h2></div><div class="gj-kv">'+kv.filter(function(x){return x[1];}).map(function(x){return '<span>'+x[0]+'</span><b>'+esc(x[1])+'</b>';}).join('')+'</div></div>'
    +'<div class="gj-card"><div class="gp-sec" style="margin:0 0 4px"><h2>History</h2></div><div class="gj-hist">'+(ev||'<div class="gp-note">No history yet.</div>')+'</div></div>');
}
// ------------------------------------------------------------------ editing
function readHead(){return {ledger_id:S.j?S.j.ledgerId:$('#gjLedger').val(),accounting_date:$('#gjDate').val(),journal_date:$('#gjJdate').val(),category:$('#gjCat').val(),
  name:$.trim($('#gjName').val()),reference_no:$.trim($('#gjRef').val()),description:$.trim($('#gjDesc').val())};}
function payload(action){var h=readHead();h.id=S.id;h.action=action;
  h.lines=S.lines.filter(function(l){return !blank(l);}).map(function(l){var o={};
    SEGORDER.forEach(function(c){o[SEGKEY[c]]=l.seg[c]?U(l.seg[c]):null;});o.dr=num(l.dr)||0;o.cr=num(l.cr)||0;o.description=l.d||null;return o;});
  return h;}
function dirty(){if(!S.dirty){S.dirty=true;}renderBar();}
$(document).on('input','#gjLb .gj-in',function(){var tr=$(this).closest('tr'),i=+tr.data('i'),l=S.lines[i];if(!l)return;
  var seg=$(this).data('seg'),f=$(this).data('f');
  if(seg){var v=U(this.value);var p=this.selectionStart;this.value=v;try{this.setSelectionRange(p,p);}catch(e){}l.seg[seg]=v;var fv=findVal(seg,v);
    $(this).toggleClass('bad',!!v&&!fv).toggleClass('ok',!!fv).attr('title',fv?fv[1]:'');}
  else if(f==='d')l.d=this.value;
  else{l[f]=this.value;if(f==='dr'&&num(this.value)){l.cr='';tr.find('[data-f=cr]').val('');}if(f==='cr'&&num(this.value)){l.dr='';tr.find('[data-f=dr]').val('');}}
  delete S.lineErr[i+1];$('#gjSub'+i).html(subHtml(l,i));dirty();});
function untouched(l){return segs().every(function(s){if(s.code==='NATURAL'||hidden(s.code))return true;var a=vals(s.code);return !l.seg[s.code]||(a.length===1&& l.seg[s.code]===a[0][0])||(s.def&& l.seg[s.code]===s.def);});}
$(document).on('focusin','#gjLb .gj-in',function(){var tr=$(this).closest('tr'),i=+tr.data('i'),l=S.lines[i],p=S.lines[i-1];
  if(!l||!p||!editable()||!untouched(l)||untouched(p))return;
  segs().forEach(function(s){if(s.code!=='NATURAL'&& p.seg[s.code]){l.seg[s.code]=p.seg[s.code];var f=findVal(s.code,l.seg[s.code]);
    tr.find('[data-seg="'+s.code+'"]').val(l.seg[s.code]).toggleClass('ok',!!f).toggleClass('bad',!f).attr('title',f?f[1]:'');}});
  if(!l.d&& p.d){l.d=p.d;tr.find('[data-f=d]').val(l.d);}
  $('#gjSub'+i).html(subHtml(l,i));});
$(document).on('blur','#gjLb .amt',function(){var n=num(this.value);if(this.value!==''&&isFinite(n)&&n>0){this.value=r2(n).toFixed(2);var i=+$(this).closest('tr').data('i');S.lines[i][$(this).data('f')]=this.value;}});
$(document).on('keydown','#gjLb .amt',function(e){if(e.key==='Enter'){e.preventDefault();var i=+$(this).closest('tr').data('i');
  if(i===S.lines.length-1){addLine();}else{$('#gjLb tr.ln[data-i='+(i+1)+'] .gj-in').first().trigger('focus');}}});
$(document).on('click','[data-del]',function(){var i=+$(this).data('del');S.lines.splice(i,1);if(!S.lines.length)S.lines.push(blankLine(null));S.lineErr={};renderLines();dirty();});
function addLine(){S.lines.push(blankLine(S.lines[S.lines.length-1]||null));renderLines();dirty();
  var tr=$('#gjLb tr.ln').last();var acc=tr.find('[data-seg=NATURAL]');(acc.length?acc:tr.find('.gj-in').first()).trigger('focus');}
$('#gjAdd').on('click',addLine);
$('#gjFill').on('click',function(){var t=totals();if(!t.diff){ok('The journal is already balanced.');return;}
  var l=S.lines[S.lines.length-1];if(num(l.dr)||num(l.cr)||l.seg.NATURAL){l=blankLine(l);S.lines.push(l);}
  if(t.diff>0){l.cr=Math.abs(t.diff).toFixed(2);l.dr='';}else{l.dr=Math.abs(t.diff).toFixed(2);l.cr='';}
  renderLines();dirty();var tr=$('#gjLb tr.ln').last();tr.find('[data-seg=NATURAL]').trigger('focus');});
$(document).on('change input','#gjHead input,#gjHead select',function(){if(this.id==='gjDate')showPeriod();if(this.id==='gjLedger'){S.lines=[blankLine(null),blankLine(null)];renderLines();showPeriod();}dirty();});
window.addEventListener('beforeunload',function(e){if(S.dirty&&editable()){e.preventDefault();e.returnValue='';}});
// ------------------------------------------------------------------ actions
function openM(title,html,okText,onOk,danger){$('#gpMt').text(title);$('#gpMc').html(html);$('#gpErr').removeClass('show').text('');
  $('#gpMok').text(okText).prop('disabled',false).toggleClass('gp-btn--danger',!!danger).toggleClass('gp-btn--pri',!danger);S.onOk=onOk;$('#gpMb').addClass('open');setTimeout(function(){$('#gpMc').find('textarea,input').first().trigger('focus');},30);}
function closeM(){$('#gpMb').removeClass('open');S.onOk=null;}
$('#gpMx,#gpMcancel').on('click',closeM);
$('#gpMb').on('click',function(e){if(e.target===this)closeM();});
$(document).on('keydown',function(e){if(e.key==='Escape'&&$('#gpMb').hasClass('open'))closeM();});
$('#gpMok').on('click',function(){if(S.onOk){$('#gpMok').prop('disabled',true);S.onOk();}});
function lineProblems(l){var m=[],miss=[],bad=[];
  segs().forEach(function(s){var v=l.seg[s.code];if(!v)miss.push(s.name);else if(!findVal(s.code,v))bad.push(s.name+' '+U(v));});
  if(miss.length)m.push('missing '+miss.join(', '));if(bad.length)m.push('not a posting value: '+bad.join(', '));
  var dr=num(l.dr),cr=num(l.cr);if(isNaN(dr)||isNaN(cr))m.push('amount is not a number');else if(dr<0||cr<0)m.push('amounts cannot be negative');
  else if(dr>0&&cr>0)m.push('a debit or a credit, not both');else if(!dr&&!cr)m.push('no amount');return m.join('; ');}
function blank(l){return !l.seg.NATURAL&&!num(l.dr)&&!num(l.cr)&&!$.trim(l.d);}
function save(action){
  var kept=S.lines.filter(function(l){return !blank(l);});if(kept.length!==S.lines.length){S.lines=kept.length?kept:[blankLine(null)];}
  S.lineErr={};var nErr=0;S.lines.forEach(function(l,i){if(blank(l))return;var m=lineProblems(l);if(m){S.lineErr[i+1]=m;nErr++;}});
  if(nErr){renderLines();notify('warn','Please check the lines',nErr+' line'+(nErr===1?' needs':'s need')+' correcting. The problem is shown in red under each line.');return;}
  var p=payload(action);
  if(!p.accounting_date){fieldErr('gjDate','Enter the accounting date.');notify('warn','Accounting date missing','Enter the date the journal belongs to.');return;}
  if(!p.name){fieldErr('gjName','Give the journal a short name, for example "October rent accrual".');notify('warn','Journal name missing','Give the journal a name so it can be found later.');return;}
  if(action==='SUBMIT'){var t=totals();if(t.diff!==0||t.dr===0){notify('warn','The journal is not balanced','Debit '+money(t.dr)+', credit '+money(t.cr)+'. You can save it as a draft now; only a balanced journal can be submitted.');return;}}
  call('J50_SAVE',{p_clob_01:JSON.stringify(p)},function(d){
      S.dirty=false;
      if(action==='SUBMIT'&& d.submitted===false){load(d.id);setTimeout(function(){notify('warn','Saved as a draft, not submitted',clean(d.submitError||''));},350);return;}
      load(d.id);setTimeout(function(){if(action==='SUBMIT')ok(S.d.fourEyes==='Y'?'Next step: another user signs in, opens it from Inbox: waiting approval and approves it.':'Next step: approve it, then post it.','Journal '+d.journalNo+' submitted');
        else ok('You can keep editing it, or submit it when it is complete.','Journal '+d.journalNo+' saved as a draft');},350);},
    function(d){S.lineErr={};(d.errors||[]).forEach(function(e){if(e.line)S.lineErr[e.line]=e.message;});renderLines();
      showErr((d.message||'')+((d.errors||[]).filter(function(e){return !e.line;}).map(function(e){return ' '+e.message;}).join('')),'The journal was not saved');});
}
var DONE={APPROVE:['Journal approved','Next step: Post to ledger, with the button on this page or from Inbox: ready to post.'],REJECT:['Journal rejected','It is back in draft so it can be corrected and submitted again.'],
  POST:['Journal posted to the ledger','The account balances of the period are updated. This journal is now final.'],
  CANCEL:['Journal cancelled','It will not be posted.'],SUBMIT:['Journal submitted','Next step: another user signs in and approves it.']};
function act(a,note){call('J50_ACTION',{x01:a,x02:S.id,x03:note||''},function(d){closeM();load(S.id);
  setTimeout(function(){if(d.warning){notify('warn',d.warningTitle||'Partly done',clean(d.warning));return;}var m=DONE[a]||['Done',''];ok(m[1],m[0]);},350);});}
$(document).on('click','#gjBar [data-act]',function(){var a=$(this).data('act'),j=S.j;
  if(a==='SAVE'){save('SAVE');return;}
  if(a==='SUBMIT'){if(S.dirty||!S.id){save('SUBMIT');}else{act('SUBMIT');}return;}
  if(a==='APPROVE'){openM('Approve journal '+j.journalNo,'<div class="gp-ready">Total '+money(j.totalDr)+' '+esc(j.currency)+' · '+esc(j.lineCount)+' lines · submitted by '+esc(j.submittedBy||j.createdBy)+'</div><div class="gp-f"><label for="gjNote">Note (optional)</label><textarea id="gjNote"></textarea></div>','Approve',function(){act('APPROVE',$('#gjNote').val());});return;}
  if(a==='REJECT'){openM('Reject journal '+j.journalNo,'<div class="gp-f"><label for="gjNote">Reason (required)</label><textarea id="gjNote" placeholder="What must be corrected?"></textarea></div>','Reject',function(){if(!$.trim($('#gjNote').val())){showErr('Enter the reason.');return;}act('REJECT',$('#gjNote').val());},true);return;}
  if(a==='POST'){openM('Post journal '+j.journalNo,'<div class="gp-ready ok"><b>What happens:</b> this is the last step. The amounts of the journal ('+money(j.totalDr)+' '+esc(j.currency)+') are added to the account balances of period '+esc(j.periodCode)+', so they appear in balances and reports. A posted journal can no longer be changed; it can only be corrected by a reversing journal.</div>','Post to ledger',function(){act('POST');});return;}
  if(a==='REVERSE'){openM('Reverse journal '+j.journalNo,'<div class="gp-ready"><b>What happens:</b> a new journal <b>REV-'+esc(j.journalNo)+'</b> is created with the same lines, debit and credit switched ('+money(j.totalDr)+' '+esc(j.currency)+'). It follows the normal steps: <b>Submit</b>, <b>Approve</b> by another user, <b>Post to ledger</b>. When it is posted, the account balances are back to what they were before this journal. This journal itself is not changed.</div>'
      +'<div class="gp-f"><label for="gjRdate">Reversal date</label><input type="date" id="gjRdate" value="'+esc(S.d.today)+'"><small class="gp-note" style="display:block;margin-top:4px">Decides the period of the reversal; the period must be open. Original date: '+esc(fd(j.accountingDate))+' (period '+esc(j.periodCode)+').</small></div>'
      +'<div class="gp-f"><label for="gjNote">Reason (required)</label><textarea id="gjNote" placeholder="Why is the journal reversed?"></textarea></div>','Create reversal',function(){
      if(!$('#gjRdate').val()){showErr('Enter the reversal date.');return;}if(!$.trim($('#gjNote').val())){showErr('Enter the reason.');return;}
      call('J50_ACTION',{x01:'REVERSE',x02:S.id,x03:$('#gjNote').val(),x04:$('#gjRdate').val()},function(d){closeM();load(d.newId);
        setTimeout(function(){ok('Next step: check it and click Submit for approval.','Reversal '+(d.newNo||'')+' created as a draft');},350);});},true);return;}
  if(a==='CANCEL'){openM('Cancel journal '+(j?j.journalNo:''),'<div class="gp-f"><label for="gjNote">Reason (required)</label><textarea id="gjNote"></textarea></div>','Cancel journal',function(){if(!$.trim($('#gjNote').val())){showErr('Enter the reason.');return;}act('CANCEL',$('#gjNote').val());},true);return;}
});
function pageId(){var v='';try{v=$v('P50_ID')||'';}catch(e){}
  if(!v){var el=document.getElementById('P50_ID');if(el)v=el.value||'';}
  if(!/^\d+$/.test(String(v))){var m=/[?&]p50_id=(\d+)/i.exec(location.search)||/P50_ID:(\d+)/i.exec(location.href);v=m?m[1]:'';}
  return v;}
$(function(){load(pageId());});
})(apex.jQuery);

(function(){
  function open(e){if(e)e.preventDefault();var m=document.getElementById('gfMb');if(!m)return;m.classList.add('open');var x=document.getElementById('gfX');if(x)x.focus();}
  function close(){var m=document.getElementById('gfMb');if(m)m.classList.remove('open');}
  document.addEventListener('click',function(e){var t=e.target.closest?e.target.closest('[data-flow]'):null;if(t){open(e);return;}
    if(e.target.id==='gfX'||e.target.id==='gfMb')close();});
  document.addEventListener('keydown',function(e){if(e.key==='Escape')close();});
})();
});
