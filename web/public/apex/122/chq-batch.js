apex.jQuery(function(){
var IC={check:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 6 9 17l-5-5"/></svg>',x:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M18 6 6 18M6 6l12 12"/></svg>',wallet:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 7H5a2 2 0 0 1 0-4h13v4"/><path d="M3 5v14a2 2 0 0 0 2 2h15V7H5"/><path d="M16 14h.01"/></svg>',refresh:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M21 12a9 9 0 1 1-3-6.7L21 8"/><path d="M21 3v5h-5"/></svg>',trash:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 6h18M8 6V4h8v2M6 6l1 14h10l1-14"/></svg>',back:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M19 12H5M12 19l-7-7 7-7"/></svg>',edit:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 20h9"/><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z"/></svg>'};IC.ask='<svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"><path d="M9.1 9a3 3 0 0 1 5.8 1c0 2-3 3-3 3M12 17h.01"/></svg>';
var SOURCES=[["FACTORING", "Factoring"], ["LEASING", "Leasing"], ["MORTGAGE", "Mortgage"], ["DIRECT", "Direct"], ["OTHER", "Other"]];

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

var S={id:pageItem('P121_BATCH_ID'),d:null,f:'ALL'};
function stName(s){s=String(s||'').toUpperCase();return {VALIDATED:'Ready to submit',HAS_ERRORS:'With errors',SUBMITTED:'Submitted',LOADED:'Loaded',DRAFT:'Draft'}[s]||lbl(s);}
function load(){apex.server.process('CHQ_BATCH',{x01:S.id},{dataType:'json'}).then(function(d){
    if(!d||d.ok===false){$('#cdHead').html('<div class="cb-empty"><b>The batch could not be opened</b>'+esc(clean(d&& d.message))+'</div>');return;}
    S.d=d;render();},function(x){$('#cdHead').html('<div class="cb-empty"><b>The batch could not be opened</b>'+esc(clean(x&& x.responseText).substring(0,300))+'</div>');});}
function canSubmit(b){return b.status!=='SUBMITTED'&&(+b.valid||0)>0&&!(+b.errors);}
function render(){var d=S.d,b=d.batch,sub=b.status==='SUBMITTED';
  $('#cdTitle').text(b.no);
  $('#cdHeroSt').html('<span class="cb-st '+esc(b.status)+'">'+esc(stName(b.status))+'</span><small>'+esc(b.file||'')+(b.created?' · created '+esc(b.created)+' by '+esc(b.createdBy||''):'')+'</small>');
  var a='<a class="gp-btn" href="'+esc(d.listUrl)+'">'+IC.back+'<span>All batches</span></a>';
  if(!sub)a+='<button type="button" class="gp-btn" data-a="VALIDATE">'+IC.refresh+'<span>Check again</span></button><button type="button" class="gp-btn gp-btn--danger" data-a="DELETE">'+IC.trash+'<span>Delete</span></button>';
  if(canSubmit(b))a+='<button type="button" class="gp-btn gp-btn--hot" data-a="SUBMIT">'+IC.wallet+'<span>Submit to the wallet</span></button>';
  $('#cdActs').html(a);
  $('#cqSteps .cq-step[data-s=4]').toggleClass('done',sub).toggleClass('now',!sub).find('i').html(sub?IC.check:'4');
  var n=$('#cdNext');
  if(sub)n.attr('class','cd-next ok').html('<span class="ni">'+IC.check+'</span><div><b>Submitted '+esc(b.submittedAt||'')+(b.submittedBy?' by '+esc(b.submittedBy):'')+'</b><p>'+esc(b.submitted)+' cheque'+(+b.submitted===1?' is':'s are')+' in the client wallet. Each line shows its cheque reference.</p></div>');
  else if(+b.errors)n.attr('class','cd-next bad').html('<span class="ni">!</span><div><b>'+esc(b.errors)+' line'+(+b.errors===1?' has':'s have')+' errors</b><p>A batch with errors cannot be submitted. Correct the Excel file and upload it again as a new batch (delete this one), or click <b>Check again</b> after a correction in the data.</p></div>');
  else if(canSubmit(b))n.attr('class','cd-next').html('<span class="ni">4</span><div><b>Ready to submit</b><p>All '+esc(b.valid)+' lines are valid. Click <b>Submit to the wallet</b> to create the cheques and put them in the client wallet.</p></div>');
  else n.attr('class','cd-next warn').html('<span class="ni">!</span><div><b>Nothing to submit yet</b><p>The batch has no valid line. Click <b>Check again</b> or upload the file again.</p></div>');
  var k=[['Lines',b.total,'#5F7B8A'],['Valid',b.valid,'#14734F'],['Errors',b.errors,'#B42318'],['Submitted',b.submitted,'#00636D'],['Total amount',money(b.amount)+' '+(b.currency||''),'#E87722']];
  $('#cdKpis').html(k.map(function(x){return '<div class="cd-k" style="--c:'+x[2]+'"><small><i></i>'+x[0]+'</small><b>'+esc(x[1]==null?0:x[1])+'</b></div>';}).join(''));
  var c=b.customer||'',ini=(c.replace(/[^A-Za-z ]/g,'').split(' ').filter(Boolean).map(function(w){return w[0];}).join('').substring(0,2)||'C').toUpperCase();
  var opt=function(list,val){var h=list.map(function(o){return '<option value="'+esc(o[0])+'"'+(o[0]===val?' selected':'')+'>'+esc(o[1])+'</option>';}).join('');
    if(val&&!list.some(function(o){return o[0]===val;}))h+='<option value="'+esc(val)+'" selected>'+esc(val)+'</option>';return h;};
  var dis=sub?' disabled':'';
  $('#cdHead').html('<h2>Batch details</h2><p>The customer comes from the Excel file.</p>'
    +'<div class="cd-cust"><span class="av">'+esc(ini)+'</span><div style="min-width:0"><b>'+esc(c||'-')+'</b><small>'+(b.customerId?'Client ID '+esc(b.customerId):'')+'</small>'+(b.customerAr?'<small class="ar">'+esc(b.customerAr)+'</small>':'')+'</div></div>'
    +'<div class="cq-f"><label for="cdCur">Currency</label><select id="cdCur"'+dis+'>'+opt(d.currencies.map(function(x){return [x.code,x.name+' ('+x.code+')'];}),b.currency)+'</select></div>'
    +'<div class="cq-f"><label for="cdLoc">Cheques are kept at</label><select id="cdLoc"'+dis+'>'+opt(d.locations.map(function(x){return [x.code,x.name];}),b.location)+'</select></div>'
    +'<div class="cq-f"><label for="cdSrc">Source module</label><select id="cdSrc"'+dis+'>'+opt(SOURCES,b.source)+'</select></div>'
    +'<div class="cq-f"><label for="cdNotes">Notes</label><input type="text" id="cdNotes" maxlength="2000" value="'+esc(b.notes||'')+'"'+dis+'></div>'
    +(sub?'':'<div class="cd-save"><button type="button" class="gp-btn gp-btn--pri" data-a="SAVE">'+IC.check+'<span>Save and check again</span></button></div>')
    +'<div class="cd-kv"><span>File</span><b>'+esc(b.file||'-')+'</b><span>Received</span><b>'+esc(b.received||'-')+'</b><span>Cheque date</span><b>'+esc(b.chequeDate||'-')+'</b>'
    +'<span>Created</span><b>'+esc((b.created||'')+(b.createdBy?' · '+b.createdBy:''))+'</b>'+(b.updated?'<span>Updated</span><b>'+esc(b.updated+(b.updatedBy?' · '+b.updatedBy:''))+'</b>':'')+'</div>');
  renderLines();}
function renderLines(){var L=S.d.lines||[],c={ALL:L.length,VALID:0,ERROR:0,SUBMITTED:0};L.forEach(function(l){if(c[l.status]!=null)c[l.status]++;});
  $('#cdChips').html([['ALL','All'],['VALID','Valid'],['ERROR','Errors'],['SUBMITTED','Submitted']].filter(function(x){return x[0]==='ALL'||c[x[0]];})
    .map(function(x){return '<button type="button" class="cd-chip'+(S.f===x[0]?' on':'')+'" data-f="'+x[0]+'">'+x[1]+' <b>'+c[x[0]]+'</b></button>';}).join(''));
  var rows=L.filter(function(l){return S.f==='ALL'||l.status===S.f;});
  if(!rows.length){$('#cdLines').html('<div class="cb-empty">No line.</div>');return;}
  $('#cdLines').html('<div class="gp-tw"><table class="cb-t cd-t"><thead><tr><th>#</th><th>Cheque no</th><th>Due date</th><th class="r">Amount</th><th>Bank</th><th>Client / tax ID</th><th>Status</th></tr></thead><tbody>'
    +rows.map(function(l){return '<tr class="'+(l.status==='ERROR'?'err':'')+'"><td>'+esc(l.no)+'</td><td><b>'+esc(l.chequeNo||l.rawChequeNo||'')+'</b>'+(l.chequeRef?'<small class="cd-ref">'+esc(l.chequeRef)+'</small>':'')+'</td>'
      +'<td>'+esc(l.dueDate?fd(l.dueDate):(l.rawDue||''))+'</td><td class="r"><b>'+(l.amount!=null?money(l.amount):esc(l.rawAmount||''))+'</b></td>'
      +'<td>'+esc(l.bank||l.rawBank||'')+(l.bankName?'<small>'+esc(l.bankName)+'</small>':'')+'</td><td>'+esc(l.clientId||l.rawClient||'')+'<small>'+esc(l.taxId||l.rawTax||'')+'</small></td>'
      +'<td><span class="cd-ls '+esc(l.status)+'">'+esc(lbl(l.status))+'</span>'+(l.error?'<span class="em">'+esc(l.error)+'</span>':'')+'</td></tr>';}).join('')+'</tbody></table></div>');}
$('#cdChips').on('click','.cd-chip',function(){S.f=$(this).data('f');renderLines();});
$(document).on('click','#cdActs [data-a],#cdHead [data-a]',function(){var a=$(this).data('a'),b=S.d.batch;
  if(a==='SAVE'){run('CHQ_BATCH_ACT',{x01:'SAVE',x02:S.id,x03:$('#cdCur').val(),x04:$('#cdLoc').val(),x05:$('#cdSrc').val(),x06:$('#cdNotes').val()},'Saving the batch…',b.no,
      ['Saving currency, location and source','Checking every line again','Counting valid lines'],function(d){after(d,'Batch saved and checked again');});return;}
  if(a==='VALIDATE'){run('CHQ_BATCH_ACT',{x01:'VALIDATE',x02:S.id},'Checking the batch again…',b.no,['Reading the '+b.total+' lines','Checking banks, dates and amounts','Checking duplicates','Counting valid lines'],
      function(d){after(d,'Batch checked again');});return;}
  if(a==='SUBMIT'){popup('ask','Submit '+b.no+' to the wallet?',b.customer||'',
      '<div class="cq-confirm"><b>What happens:</b> '+esc(b.valid)+' cheque'+(+b.valid===1?' is':'s are')+' created for '+esc(b.customer||'the customer')+' ('+money(b.amount)+' '+esc(b.currency||'')+'), each with its cheque reference, '
      +'and put in the client wallet with a first "received" movement. The batch can no longer be changed.</div>',
      [['close','Cancel'],['go','Submit to the wallet','gp-btn--hot','wallet']],function(k){if(k!=='go')return;
        run('CHQ_BATCH_ACT',{x01:'SUBMIT',x02:S.id},'Submitting to the wallet…',b.no,['Checking the batch one last time','Creating '+b.valid+' cheque'+(+b.valid===1?'':'s'),'Recording the first movement','Putting the cheques in the client wallet'],
          function(d){if(d.ok===false){fail(d,'The batch was not submitted');return;}
            popup('ok',d.count+' cheque'+(d.count===1?'':'s')+' in the wallet','Batch '+b.no+' is submitted.',
              '<ul class="cq-cl"><li class="ok" style="--i:0"><span class="mk">'+IC.check+'</span><div class="grow"><b>Cheques created</b><p>'+esc(d.count)+' cheque'+(d.count===1?'':'s')+' for '+esc(b.customer||'')+'. Their references are on the lines.</p></div></li></ul>',
              [['close','Close'],['wallet','Open the cheques wallet','gp-btn--hot','wallet']],function(k){if(k==='wallet'&& S.d.walletUrl)apex.navigation.redirect(S.d.walletUrl);});load();});});return;}
  if(a==='DELETE'){popup('bad','Delete batch '+b.no+'?','This cannot be undone.','<div class="cq-confirm danger">The batch and its '+esc(b.total)+' lines are deleted. No cheque was created from it, so nothing else changes.</div>',
      [['close','Cancel'],['go','Delete the batch','gp-btn--danger','trash']],function(k){if(k!=='go')return;
        run('CHQ_BATCH_ACT',{x01:'DELETE',x02:S.id},'Deleting the batch…',b.no,['Removing the lines','Removing the batch'],function(d){if(d.ok===false){fail(d,'The batch was not deleted');return;}
          notify('ok','Batch '+b.no+' deleted','');setTimeout(function(){apex.navigation.redirect(S.d.listUrl);},900);});});return;}});
function fail(d,title){popup('bad',title,'Nothing was changed.','<ul class="cq-cl"><li class="bad" style="--i:0"><span class="mk">'+IC.x+'</span><div class="grow"><b>Reason</b><p>'+esc(clean(d.message))+'</p></div></li></ul>',[['close','Close']]);}
function after(d,title){if(d.ok===false){fail(d,'Not done');return;}var b=d.batch||{};
  popup(+b.errors?'bad':'ok',title,(+b.valid||0)+' valid line'+(+b.valid===1?'':'s')+(+b.errors?', '+b.errors+' with errors':'')+'.','',[['close','Close']]);load();}
load();
});
