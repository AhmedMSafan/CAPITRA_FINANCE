apex.jQuery(function(){
var IC={check:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 6 9 17l-5-5"/></svg>',x:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M18 6 6 18M6 6l12 12"/></svg>',wallet:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 7H5a2 2 0 0 1 0-4h13v4"/><path d="M3 5v14a2 2 0 0 0 2 2h15V7H5"/><path d="M16 14h.01"/></svg>',refresh:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M21 12a9 9 0 1 1-3-6.7L21 8"/><path d="M21 3v5h-5"/></svg>',trash:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 6h18M8 6V4h8v2M6 6l1 14h10l1-14"/></svg>',back:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M19 12H5M12 19l-7-7 7-7"/></svg>',edit:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 20h9"/><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z"/></svg>',move:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M17 1l4 4-4 4"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/><path d="M7 23l-4-4 4-4"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/></svg>'};IC.ask=IC.edit;
var TO='<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg>',COLORS=["#00636D", "#E87722", "#5B5BD6", "#14734F", "#0EA5E9", "#B4570E", "#7C3AED", "#64748B"],FIELDS=[["Cheque", "chequeNo", "Cheque no", "text"], ["Cheque", "chequeDate", "Cheque date", "date"], ["Cheque", "dueIso", "Due date", "date"], ["Cheque", "amount", "Amount", "number"], ["Cheque", "currency", "Currency", "select:currencies"], ["Cheque", "legalEntity", "Legal entity", "select:entities"], ["Issuer bank", "bank", "Bank", "select:banks"], ["Issuer bank", "bankBranch", "Branch", "text"], ["Issuer bank", "issuerAccount", "Account no", "text"], ["Drawer & customer", "drawer", "Drawer name", "text"], ["Drawer & customer", "drawerAr", "Drawer name (Arabic)", "text"], ["Drawer & customer", "drawerId", "Drawer national ID", "text"], ["Drawer & customer", "drawerAccount", "Drawer account no", "text"], ["Drawer & customer", "customerId", "Client ID", "text"], ["Drawer & customer", "taxId", "Party tax ID", "text"], ["Drawer & customer", "customer", "Customer name", "text"], ["Drawer & customer", "customerAr", "Customer name (Arabic)", "text"], ["Source & links", "source", "Source module", "select:sources"], ["Source & links", "refType", "Source ref type", "text"], ["Source & links", "refId", "Source ref ID", "text"], ["Source & links", "drawdown", "Drawdown", "select:drawdowns"], ["Source & links", "schedule", "Repayment schedule", "select:schedules"], ["Source & links", "payment", "Payment", "select:payments"], ["Status & custody", "status", "Status", "select:statuses"], ["Status & custody", "loc", "Location", "select:locations"], ["Status & custody", "custodian", "Custodian", "text"], ["Status & custody", "depositAccount", "Deposit bank account", "select:accounts"], ["Status & custody", "depositBatch", "Deposit batch ID", "number"], ["Status & custody", "receivedIso", "Received", "date"], ["Status & custody", "depositIso", "Deposited", "date"], ["Status & custody", "collectionIso", "Collected", "date"], ["Status & custody", "rejectionIso", "Rejected", "date"], ["Status & custody", "withdrawalIso", "Withdrawn", "date"], ["Status & custody", "reason", "Rejection reason", "select:reasons"], ["Status & custody", "rejectionNotes", "Rejection notes", "text"], ["Status & custody", "replaced", "Replaced (Y/N)", "select:yn"], ["Status & custody", "replacementId", "Replacement cheque ID", "number"], ["Notes", "notes", "Notes", "text"]],SOURCES=[["FACTORING", "Factoring"], ["LEASING", "Leasing"], ["MORTGAGE", "Mortgage"], ["DIRECT", "Direct"], ["OTHER", "Other"]];

var MINWAIT=3000;
function esc(v){return apex.util.escapeHTML(v==null?'':String(v));}
function money(n){n=+n||0;return n.toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2});}
function lbl(v){v=String(v||'').replace(/_/g,' ').toLowerCase();return v.replace(/\b\w/g,function(c){return c.toUpperCase();});}
function fd(d){if(!d)return '';var p=String(d).split('-');var m=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];return p.length<3?d:(+p[2])+' '+m[+p[1]-1]+' '+p[0];}
function clean(m){return String(m||'').replace(/^ORA-\d+:\s*/,'').replace(/\nORA-[\s\S]*$/,'');}
var GN_ICO={ok:IC.check,err:'<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round"><path d="M12 7v6M12 17h.01"/></svg>'};
GN_ICO.warn=GN_ICO.err;GN_ICO.info=GN_ICO.err;
function notify(kind,title,msg,ms){var w=document.getElementById('gnWrap');if(!w){w=document.createElement('div');w.id='gnWrap';w.className='gn-wrap';w.setAttribute('aria-live','polite');document.body.appendChild(w);}
  ms=ms||(kind==='ok'?4500:(kind==='err'?12000:7000));
  var n=document.createElement('div');n.className='gn '+kind;n.setAttribute('role',kind==='err'?'alert':'status');n.style.setProperty('--d',ms+'ms');
  n.innerHTML='<span class="gn-i">'+GN_ICO[kind]+'</span><div class="gn-b"><b>'+esc(title)+'</b>'+(msg?'<p>'+esc(msg)+'</p>':'')+'</div><button type="button" class="gn-x" aria-label="Close">&times;</button><i class="gn-t"></i>';
  function close(){if(!n.parentNode)return;n.classList.add('out');setTimeout(function(){if(n.parentNode)n.parentNode.removeChild(n);},230);}
  n.querySelector('.gn-x').addEventListener('click',close);setTimeout(close,ms);
  while(w.children.length>=3)w.removeChild(w.firstChild);w.appendChild(n);}
function open(id){$('#'+id).addClass('open').attr('aria-hidden','false');}
function close(id){$('#'+id).removeClass('open').attr('aria-hidden','true');}
var BUSY=false;
$(document).on('keydown',function(e){if(e.key==='Escape'&&$('#cqRes').hasClass('open')&&!BUSY)close('cqRes');});
$('#cqRx').on('click',function(){if(!BUSY)close('cqRes');});
$('#cqRes').on('click',function(e){if(e.target===this&&!BUSY)close('cqRes');});
var tick=null;
function loading(title,sub,steps){$('#cqLt').text(title);$('#cqLf').text(sub||'');
  $('#cqLs').html(steps.map(function(t){return '<li><span class="st"></span><span>'+esc(t)+'</span></li>';}).join(''));
  var items=$('#cqLs li'),i=0;$('#cqBar').css('width','0');open('cqLoad');
  function next(){if(i>0)items.eq(i-1).removeClass('run').addClass('ok').find('.st').html(IC.check);
    if(i<items.length){items.eq(i).addClass('run');$('#cqBar').css('width',Math.round((i+1)/items.length*92)+'%');i++;tick=setTimeout(next,MINWAIT/items.length);}}
  next();}
function stopLoading(){clearTimeout(tick);$('#cqLs li').removeClass('run').addClass('ok').find('.st').html(IC.check);$('#cqBar').css('width','100%');}
// popup: kind ok | bad | ask ; body html ; buttons [[key,label,cls,icon]]
function popup(kind,title,sub,body,buttons,onBtn){$('#cqRerr').removeClass('show').text('');
  $('#cqRbig').attr('class','big '+(kind==='bad'?'bad':'ok')).html(kind==='bad'?IC.x:(kind==='ask'?IC.ask:IC.check));
  $('#cqRt').text(title);$('#cqRs').text(sub||'');$('#cqRc').html(body||'');
  $('#cqRf').html(buttons.map(function(b){return '<button type="button" class="gp-btn '+(b[2]||'')+'" data-b="'+b[0]+'">'+(b[3]?IC[b[3]]:'')+'<span>'+esc(b[1])+'</span></button>';}).join(''));
  $('#cqRf').off('click').on('click','[data-b]',function(){var k=$(this).data('b');if(k==='close'){close('cqRes');return;}onBtn&&onBtn(k,this);});
  open('cqRes');}
// run a server action with the animated popup (at least 3 seconds), then call done(result)
function run(proc,data,title,sub,steps,done){BUSY=true;close('cqRes');var t0=Date.now();loading(title,sub,steps);
  apex.server.process(proc,data,{dataType:'json'}).then(fin,function(x){fin({ok:false,message:clean(x&& x.responseText)||'The request failed.'});});
  function fin(d){var w=Math.max(0,MINWAIT-(Date.now()-t0));setTimeout(function(){stopLoading();setTimeout(function(){close('cqLoad');BUSY=false;done(d||{});},350);},w);}}
function pageItem(name){var v='';try{v=$v(name)||'';}catch(e){}if(!v){var el=document.getElementById(name);if(el)v=el.value||'';}
  if(!/^\d+$/.test(String(v))){var m=new RegExp('[?&]'+name+'=(\\d+)','i').exec(location.search)||new RegExp(name+':(\\d+)','i').exec(location.href);v=m?m[1]:'';}return v;}

function due(iso){if(!iso)return {d:null};var p=iso.split('-'),d=new Date(+p[0],+p[1]-1,+p[2]),t=new Date();t.setHours(0,0,0,0);return {d:Math.round((d-t)/86400000)};}
function dueBadge(r){if(!r.dueIso||r.terminal==='Y')return '';var n=due(r.dueIso).d;
  if(n<0)return '<span class="cw-due over">'+(-n)+' day'+(n===-1?'':'s')+' overdue</span>';
  if(n===0)return '<span class="cw-due soon">due today</span>';
  if(n<=30)return '<span class="cw-due soon">in '+n+' day'+(n===1?'':'s')+'</span>';return '<span class="cw-due later">in '+n+' days</span>';}

var S={id:pageItem('P131_CHEQUE_ID'),d:null};
function load(){apex.server.process('CHQ_CHEQUE',{x01:S.id},{dataType:'json'}).then(function(d){
    if(!d||d.ok===false){$('#ccInfo').html('<div class="cb-empty"><b>The cheque could not be opened</b>'+esc(clean(d&& d.message))+'</div>');return;}S.d=d;render();},
  function(x){$('#ccInfo').html('<div class="cb-empty"><b>The cheque could not be opened</b>'+esc(clean(x&& x.responseText).substring(0,300))+'</div>');});}
function kv(rows){rows=rows.filter(function(r){return r[1]!=null&&r[1]!=='';});if(!rows.length)return '';
  return '<div class="cc-kv">'+rows.map(function(r){return '<span>'+r[0]+'</span><b'+(r[2]?' class="'+r[2]+'"':'')+'>'+esc(r[1])+'</b>';}).join('')+'</div>';}
function sec(t,rows){var h=kv(rows);return h?'<div class="cc-sec"><h3>'+t+'</h3>'+h+'</div>':'';}
function render(){var d=S.d,c=d.cheque,E=d.events||[];
  $('#ccTitle').text(c.ref);
  $('#ccHeroSt').html('<span class="cb-st SUBMITTED">'+esc(c.statusName||c.status)+'</span><small>Cheque '+esc(c.chequeNo||'')+' · '+esc(c.bankName||c.bank||'')+'</small>');
  $('#ccAmt').html(money(c.amount)+'<small>'+esc(c.currency||'')+'</small>');
  $('#ccActs').html('<a class="gp-btn" href="'+esc(d.listUrl)+'">'+IC.back+'<span>All cheques</span></a><button type="button" class="gp-btn" data-a="EDIT">'+IC.edit+'<span>Edit</span></button>'
    +(E.length?'':'<button type="button" class="gp-btn gp-btn--danger" data-a="DELETE">'+IC.trash+'<span>Delete</span></button>'));
  var r={dueIso:c.dueIso,terminal:c.terminal};
  $('#ccHd').html([['Due date',c.due||'-',dueBadge(r)],['Customer',c.customer||'-',c.customerId?'Client ID '+esc(c.customerId):''],['Location',c.locName||c.loc||'-',esc(c.custodian||'')],
    ['Batch',c.batchNo||'-',c.batchUrl?'<a href="'+esc(c.batchUrl)+'" style="color:#00636D;font-weight:700">Open the batch</a>':'']].map(function(x){return '<div><small>'+x[0]+'</small><b>'+esc(x[1])+'</b><span>'+x[2]+'</span></div>';}).join(''));
  $('#ccInfo').html('<h2>Cheque details</h2>'
    +sec('Cheque',[['Cheque ref',c.ref],['Cheque no',c.chequeNo],['Cheque date',c.chequeDateTxt],['Due date',c.due],['Amount',money(c.amount)+' '+(c.currency||'')],['Legal entity',c.legalEntityName]])
    +sec('Issuer bank',[['Bank',(c.bank||'')+(c.bankName?' · '+c.bankName:'')],['Branch',c.bankBranch],['Account no',c.issuerAccount]])
    +sec('Drawer',[['Name',c.drawer],['Arabic name',c.drawerAr,'ar'],['National ID',c.drawerId],['Account no',c.drawerAccount]])
    +sec('Customer',[['Name',c.customer],['Arabic name',c.customerAr,'ar'],['Client ID',c.customerId],['Party tax ID',c.taxId]])
    +sec('Source & links',[['Source module',c.source],['Source ref',[c.refType,c.refId].filter(Boolean).join(' ')],['Drawdown',c.drawdownName],['Schedule',c.schedule],['Payment',c.paymentName]])
    +sec('Bank deposit & result',[['Deposit account',c.depositAccountName],['Deposit batch',c.depositBatchRef],['Received',c.received],['Deposited',c.deposit],['Collected',c.collection],['Rejected',c.rejection],['Withdrawn',c.withdrawal],
        ['Rejection reason',c.reasonName],['Rejection notes',c.rejectionNotes],['Replaced',c.replaced==='Y'?'Yes'+(c.replacementId?' · by cheque #'+c.replacementId:''):'']])
    +sec('Accounting',[['GL posted',c.glPosted],['Journal ref',c.glRef],['Posted on',c.glDate]])
    +sec('Notes',[['Notes',c.notes]])
    +sec('Audit',[['Created',(c.created||'')+(c.createdBy?' · '+c.createdBy:'')],['Updated',c.updated?(c.updated+(c.updatedBy?' · '+c.updatedBy:'')):'']]));
  $('#ccN').text(E.length+' movement'+(E.length===1?'':'s'));
  if(!E.length){$('#ccTl').html('<div class="cb-empty"><b>No movement yet</b>The movements of this cheque appear here.</div>');return;}
  var types=[];E.forEach(function(e){if(types.indexOf(e.type)<0)types.push(e.type);});
  $('#ccTl').html('<ul class="cc-tl">'+E.map(function(e){var col=COLORS[types.indexOf(e.type)%COLORS.length];
    return '<li><span class="dot" style="--c:'+col+'">'+IC.move+'</span><div class="bd"><div class="r1"><b>'+esc(e.typeName||e.type)+'</b><small>'+esc(e.date||'')+'</small></div>'
      +((e.from||e.to)?'<div class="cm-path"><span>'+esc(e.fromName||e.from||'-')+'</span>'+TO+'<span class="to">'+esc(e.toName||e.to||'-')+'</span></div>':'')
      +'<small>'+[e.oldStatusName||e.oldStatus?('status '+(e.oldStatusName||e.oldStatus||'')+' → '+(e.newStatusName||e.newStatus||'')):'',e.handover?'handed over by '+e.handover:'',e.receivedBy?'received by '+e.receivedBy:'',e.by?'recorded by '+e.by:''].filter(Boolean).map(esc).join(' · ')+'</small>'
      +(e.bankRef||e.reason||e.amount?'<small>'+[e.amount?money(e.amount)+' '+(e.currency||''):'',e.bankRef?'bank ref '+e.bankRef:'',e.reason?'reason: '+e.reason:''].filter(Boolean).map(esc).join(' · ')+'</small>':'')
      +(e.notes?'<p>'+esc(e.notes)+'</p>':'')+(e.moveUrl?'<p><a href="'+esc(e.moveUrl)+'">Movement '+esc(e.moveRef||('#'+e.moveId))+'</a></p>':'')+'</div></li>';}).join('')+'</ul>');}
function opts(list,val,blank){var h=blank?'<option value=""></option>':'';h+=(list||[]).map(function(o){return '<option value="'+esc(o[0])+'"'+(String(o[0])===String(val==null?'':val)?' selected':'')+'>'+esc(o[1])+'</option>';}).join('');
  if(val!=null&&val!==''&&!(list||[]).some(function(o){return String(o[0])===String(val);}))h+='<option value="'+esc(val)+'" selected>'+esc(val)+'</option>';return h;}
function editForm(){var c=S.d.cheque,L=S.d.lists,sec='',h='<div class="cc-form">';
  FIELDS.forEach(function(f){if(f[0]!==sec){sec=f[0];h+='<h4>'+esc(sec)+'</h4>';if(sec==='Status & custody')h+='<div class="cc-warn">These are normally changed by the movements (receive, transfer, deposit, collect, reject). Change them here only to correct a mistake.</div>';}
    var v=c[f[1]],id='ccF_'+f[1],t=f[3],inp;
    if(t.indexOf('select:')===0){var key=t.substring(7),list=key==='yn'?[['Y','Yes'],['N','No']]:key==='sources'?SOURCES:L[key];inp='<select id="'+id+'">'+opts(list,v,true)+'</select>';}
    else inp='<input id="'+id+'" type="'+t+'"'+(t==='number'?' step="any"':'')+(f[5]?' maxlength="'+f[5]+'"':'')+' value="'+esc(v==null?'':v)+'">';
    h+='<div class="cq-f'+(f[1]==='notes'||f[1]==='rejectionNotes'?' span2':'')+'"><label for="'+id+'">'+esc(f[2])+'</label>'+inp+'</div>';});
  return h+'</div>';}
$(document).on('click','#ccActs [data-a]',function(){var a=$(this).data('a'),c=S.d.cheque;
  if(a==='EDIT'){popup('ask','Edit cheque '+c.ref,'Correct the cheque details.',editForm(),[['close','Cancel'],['save','Save','gp-btn--pri','check']],function(k){if(k!=='save')return;
      var o={};FIELDS.forEach(function(f){o[f[1]]=$('#ccF_'+f[1]).val();});
      if(!o.chequeNo||!o.dueIso||!o.amount||!o.currency){$('#cqRerr').text('Cheque no, due date, amount and currency are required.').addClass('show');return;}
      run('CHQ_CHEQUE_ACT',{x01:'SAVE',x02:S.id,p_clob_01:JSON.stringify(o)},'Saving the cheque…',c.ref,['Saving the cheque details'],function(d){
        if(d.ok===false){popup('bad','Not saved','Nothing was changed.','<div class="cq-confirm danger">'+esc(clean(d.message))+'</div>',[['close','Close']]);return;}notify('ok','Cheque '+c.ref+' saved','');load();});});
    $('#cqRbig').attr('class','big ok').html(IC.edit);return;}
  if(a==='DELETE'){popup('bad','Delete cheque '+c.ref+'?','It has no movement.','<div class="cq-confirm danger">The cheque record is deleted. This cannot be undone.</div>',[['close','Cancel'],['go','Delete','gp-btn--danger','trash']],function(k){if(k!=='go')return;
      run('CHQ_CHEQUE_ACT',{x01:'DELETE',x02:S.id},'Deleting the cheque…',c.ref,['Removing the cheque'],function(d){if(d.ok===false){popup('bad','Not deleted','','<div class="cq-confirm danger">'+esc(clean(d.message))+'</div>',[['close','Close']]);return;}
        notify('ok','Cheque deleted','');setTimeout(function(){apex.navigation.redirect(S.d.listUrl);},900);});});}});
load();
});
