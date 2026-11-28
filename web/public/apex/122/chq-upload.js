apex.jQuery(function(){
var IC={check:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 6 9 17l-5-5"/></svg>',x:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M18 6 6 18M6 6l12 12"/></svg>',wallet:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 7H5a2 2 0 0 1 0-4h13v4"/><path d="M3 5v14a2 2 0 0 0 2 2h15V7H5"/><path d="M16 14h.01"/></svg>',refresh:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M21 12a9 9 0 1 1-3-6.7L21 8"/><path d="M21 3v5h-5"/></svg>'};
var CHECKS=[["file", "Excel file can be read"], ["complete", "No empty cells"], ["banks", "Issuer bank on every cheque"], ["customer", "One customer per file"], ["crm", "Customer exists in CRM"], ["values", "Cheque number, due date and amount are valid"], ["dupes", "No duplicate cheques"]];
var MINWAIT=3000;
var S={d:null,file:null,b64:null,res:null,busy:false};
function esc(v){return apex.util.escapeHTML(v==null?'':String(v));}
function money(n){n=+n||0;return n.toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2});}
function fd(d){if(!d)return '';var p=String(d).split('-');var m=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];return p.length<3?d:(+p[2])+' '+m[+p[1]-1]+' '+p[0];}
function size(n){return n<1024?n+' bytes':(n<1048576?(n/1024).toFixed(1)+' KB':(n/1048576).toFixed(2)+' MB');}
function clean(m){return String(m||'').replace(/^ORA-\d+:\s*/,'').replace(/\nORA-[\s\S]*$/,'');}
// ---------------------------------------------------------------- toasts (same as the accounting pages)
var GN_ICO={ok:IC.check,err:'<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round"><path d="M12 7v6M12 17h.01"/></svg>'};
GN_ICO.warn=GN_ICO.err;GN_ICO.info=GN_ICO.err;
function notify(kind,title,msg,ms){var w=document.getElementById('gnWrap');if(!w){w=document.createElement('div');w.id='gnWrap';w.className='gn-wrap';w.setAttribute('aria-live','polite');document.body.appendChild(w);}
  ms=ms||(kind==='ok'?4500:(kind==='err'?12000:7000));
  var n=document.createElement('div');n.className='gn '+kind;n.setAttribute('role',kind==='err'?'alert':'status');n.style.setProperty('--d',ms+'ms');
  n.innerHTML='<span class="gn-i">'+GN_ICO[kind]+'</span><div class="gn-b"><b>'+esc(title)+'</b>'+(msg?'<p>'+esc(msg)+'</p>':'')+'</div><button type="button" class="gn-x" aria-label="Close">&times;</button><i class="gn-t"></i>';
  function close(){if(!n.parentNode)return;n.classList.add('out');setTimeout(function(){if(n.parentNode)n.parentNode.removeChild(n);},230);}
  n.querySelector('.gn-x').addEventListener('click',close);setTimeout(close,ms);
  while(w.children.length>=3)w.removeChild(w.firstChild);w.appendChild(n);}
// ---------------------------------------------------------------- steps strip
function step(n){$('#cqSteps .cq-step').each(function(){var s=+$(this).data('s');$(this).toggleClass('done',s<n).toggleClass('now',s===n);
  $(this).find('i').html(s<n?IC.check:String(s));});}
// ---------------------------------------------------------------- init
function init(){apex.server.process('CHQ_INIT',{},{dataType:'json'}).then(function(d){S.d=d||{};
    $('#cqCur').html((d.currencies||[]).map(function(c){return '<option value="'+esc(c.code)+'"'+(c.code===d.currency?' selected':'')+'>'+esc(c.name)+' ('+esc(c.code)+')</option>';}).join(''));
    $('#cqLoc').html((d.locations||[]).map(function(c){return '<option value="'+esc(c.code)+'"'+(c.code===d.location?' selected':'')+'>'+esc(c.name)+'</option>';}).join(''));
    if(d.source)$('#cqSrc').val(d.source);
    $('#cqBatches').attr('href',d.batchesUrl||'#');},
  function(x){notify('err','The page could not load',clean(x&& x.responseText));});}
// ---------------------------------------------------------------- file choice
function pick(f){if(!f)return;
  if(!/\.xlsx$/i.test(f.name)){notify('warn','Not an Excel .xlsx file',/\.xls$/i.test(f.name)?'This is an old .xls file. Open it in Excel and save it as "Excel Workbook (*.xlsx)".':'Choose the .xlsx file made from the template.');return;}
  if(f.size>5*1048576){notify('warn','The file is too large','The limit is 5 MB.');return;}
  S.file=f;S.b64=null;$('#cqFname').text(f.name);$('#cqFsize').text(size(f.size)+' · ready to check');$('#cqFile').addClass('show');$('#cqCheck').prop('disabled',false);
  var r=new FileReader();r.onload=function(){var s=String(r.result||'');S.b64=s.substring(s.indexOf(',')+1);};r.readAsDataURL(f);}
function clearFile(){S.file=null;S.b64=null;S.res=null;$('#cqInput').val('');$('#cqFile').removeClass('show');$('#cqCheck').prop('disabled',true);step(1);}
$('#cqInput').on('change',function(){pick(this.files&& this.files[0]);});
$('#cqDrop').on('keydown',function(e){if(e.key==='Enter'||e.key===' '){e.preventDefault();$('#cqInput').trigger('click');}})
  .on('dragenter dragover',function(e){e.preventDefault();$(this).addClass('over');})
  .on('dragleave dragend drop',function(e){e.preventDefault();$(this).removeClass('over');})
  .on('drop',function(e){var dt=e.originalEvent.dataTransfer;pick(dt&& dt.files&& dt.files[0]);});
$('#cqClear').on('click',clearFile);
// ---------------------------------------------------------------- popups
function open(id){$('#'+id).addClass('open').attr('aria-hidden','false');}
function close(id){$('#'+id).removeClass('open').attr('aria-hidden','true');}
$(document).on('keydown',function(e){if(e.key==='Escape'&&$('#cqRes').hasClass('open')&&!S.busy)close('cqRes');});
$('#cqRx').on('click',function(){if(!S.busy)close('cqRes');});
$('#cqRes').on('click',function(e){if(e.target===this&&!S.busy)close('cqRes');});
// the loading popup: one line of the list lights up every half second, so the 3 seconds show what is being checked
var tick=null;
function loading(title,steps){steps=steps||CHECKS.map(function(c){return c[1];});
  $('#cqLt').text(title||'Checking your file…');
  $('#cqLs').html(steps.map(function(t){return '<li><span class="st"></span><span>'+esc(t)+'</span></li>';}).join(''));
  var items=$('#cqLs li'),i=0;$('#cqLf').text(S.file.name+' · '+size(S.file.size));$('#cqBar').css('width','0');open('cqLoad');
  function next(){if(i>0)items.eq(i-1).removeClass('run').addClass('ok').find('.st').html(IC.check);
    if(i<items.length){items.eq(i).addClass('run');$('#cqBar').css('width',Math.round((i+1)/items.length*92)+'%');i++;tick=setTimeout(next,MINWAIT/items.length);}}
  next();}
function stopLoading(){clearTimeout(tick);$('#cqLs li').removeClass('run').addClass('ok').find('.st').html(IC.check);$('#cqBar').css('width','100%');}
function waitB64(cb,n){n=n||0;if(S.b64)return cb();if(n>100){notify('err','The file could not be read','Choose it again.');return;}setTimeout(function(){waitB64(cb,n+1);},50);}
// ---------------------------------------------------------------- 2. check
$('#cqCheck').on('click',function(){if(!S.file||S.busy)return;waitB64(function(){
  S.busy=true;step(2);loading();var t0=Date.now();
  apex.server.process('CHQ_CHECK',{x01:S.file.name,p_clob_01:S.b64},{dataType:'json'}).then(function(d){done(d);},function(x){done({ok:false,message:clean(x&& x.responseText)||'The check could not run.'});});
  function done(d){var wait=Math.max(0,MINWAIT-(Date.now()-t0));setTimeout(function(){stopLoading();setTimeout(function(){close('cqLoad');S.busy=false;S.res=d;result(d);},350);},wait);}
});});
// ---------------------------------------------------------------- result popup: a green tick or a red cross per check
function result(d){
  $('#cqRerr').removeClass('show').text('');
  if(!d||d.ok===false){$('#cqRbig').attr('class','big bad').html(IC.x);$('#cqRt').text('The file could not be checked');$('#cqRs').text(S.file?S.file.name:'');
    $('#cqRc').html('<ul class="cq-cl"><li class="bad" style="--i:0"><span class="mk">'+IC.x+'</span><div class="grow"><b>Error</b><p>'+esc(clean(d&& d.message))+'</p></div></li></ul>');
    $('#cqRf').html('<button type="button" class="gp-btn" data-r="close">Close</button>');step(1);open('cqRes');return;}
  var checks=d.checks||[],bad=checks.filter(function(c){return !c.ok;}),pass=!bad.length,c=d.customer||{};
  $('#cqRbig').attr('class','big '+(pass?'ok':'bad')).html(pass?IC.check:IC.x);
  $('#cqRt').text(pass?'All checks passed':(bad.length===1?'1 check failed':bad.length+' checks failed'));
  $('#cqRs').text(pass?'The file is ready. Create the batch to load the cheques for this customer.':'Nothing was saved. Correct the file and check it again.');
  var h='';
  if(d.lineCount){h+='<div class="cq-sum">'
    +'<div><small>Customer</small><b>'+esc(c.name||(c.id?'Client '+c.id:'-'))+'</b><span>'+esc(c.id?('Client ID '+c.id+(c.nameAr?' · '+c.nameAr:'')):(c.ids||''))+'</span></div>'
    +'<div><small>Cheques</small><b>'+esc(d.lineCount)+'</b><span>'+esc(S.file?S.file.name:'')+'</span></div>'
    +'<div><small>Total amount</small><b>'+money(d.total)+'</b><span>'+esc($('#cqCur').val()||'')+'</span></div>'
    +'<div><small>Due dates</small><b>'+esc(fd(d.firstDue)||'-')+'</b><span>'+(d.lastDue&& d.lastDue!==d.firstDue?'to '+esc(fd(d.lastDue)):'')+'</span></div></div>';}
  h+='<ul class="cq-cl">'+checks.map(function(k,i){var t=CHECKS.filter(function(x){return x[0]===k.key;})[0];
    return '<li class="'+(k.ok?'ok':'bad')+'" style="--i:'+i+'"><span class="mk">'+(k.ok?IC.check:IC.x)+'</span><div class="grow"><b>'+esc(t?t[1]:k.key)+'</b><p>'+esc(k.detail||'')+'</p>'
      +((k.issues||[]).length?'<ul>'+k.issues.map(function(s){return '<li>'+esc(s)+'</li>';}).join('')+(k.more?'<li>and '+esc(k.more)+' more</li>':'')+'</ul>':'')+'</div></li>';}).join('')+'</ul>';
  var ln=d.lines||[];
  if(ln.length){h+='<details class="cq-lines"'+(pass?'':' open')+'><summary>Show the '+ln.length+' line'+(ln.length===1?'':'s')+' of the file</summary><div class="gp-tw"><table class="cq-lt"><thead><tr><th>Line</th><th>Client ID</th><th>Cheque no</th><th>Due date</th><th class="r">Amount</th><th>Bank</th><th></th></tr></thead><tbody>'
    +ln.map(function(l){var e=(l.errors||[]);return '<tr class="'+(e.length?'bad':'')+'"><td>'+esc(l.line)+'</td><td>'+esc(l.clientId)+'</td><td>'+esc(l.chequeNo)+'</td><td>'+esc(l.dueDate?fd(l.dueDate):l.dueRaw)+'</td>'
      +'<td class="r">'+(l.amount!=null?money(l.amount):esc(l.amountRaw))+'</td><td>'+esc(l.bank)+(l.bankName?'<span class="e" style="color:#5F7B8A">'+esc(l.bankName)+'</span>':'')+'</td>'
      +'<td>'+(e.length?'<span class="no">&#10005;</span>'+e.map(function(x){return '<span class="e">'+esc(x)+'</span>';}).join(''):'<span class="yes">&#10003;</span>')+'</td></tr>';}).join('')
    +'</tbody></table></div></details>';}
  $('#cqRc').html(h);
  $('#cqRf').html(pass?'<button type="button" class="gp-btn" data-r="close">Cancel</button><button type="button" class="gp-btn gp-btn--hot" data-r="create">'+IC.wallet+'<span>Create the batch</span></button>'
                      :'<button type="button" class="gp-btn" data-r="close">Close</button><button type="button" class="gp-btn gp-btn--pri" data-r="again">'+IC.refresh+'<span>Choose another file</span></button>');
  step(pass?3:1);open('cqRes');
}
$('#cqRf').on('click','[data-r]',function(){var a=$(this).data('r');
  if(a==='close'){close('cqRes');return;}
  if(a==='again'){close('cqRes');clearFile();$('#cqInput').trigger('click');return;}
  if(a==='create')create(this);
  if(a==='open'){if(S.created&& S.created.url)apex.navigation.redirect(S.created.url);return;}
  if(a==='another'){close('cqRes');clearFile();S.created=null;return;}});
// ---------------------------------------------------------------- 3. create the batch
// same animated popup (at least 3 seconds), then a popup with the result
function create(btn){if(S.busy)return;S.busy=true;var r=S.res||{},c=r.customer||{},t0=Date.now();
  close('cqRes');
  loading('Creating the batch…',['Checking the file again','Creating the batch for '+(c.name||('Client '+(c.id||''))),'Loading '+(r.lineCount||'')+' cheque line'+(r.lineCount===1?'':'s'),'Validating every line','Batch ready to submit']);
  apex.server.process('CHQ_CREATE',{x01:S.file.name,x02:$('#cqCur').val(),x03:$('#cqLoc').val(),x04:$('#cqSrc').val(),x05:$('#cqNotes').val(),p_clob_01:S.b64},{dataType:'json'}).then(function(d){done(d);},
    function(x){done({ok:false,message:clean(x&& x.responseText)||'The batch was not created.'});});
  function done(d){var wait=Math.max(0,MINWAIT-(Date.now()-t0));setTimeout(function(){stopLoading();setTimeout(function(){close('cqLoad');S.busy=false;created(d);},350);},wait);}}
function created(d){var r=S.res||{},c=r.customer||{};$('#cqRerr').removeClass('show').text('');
  if(!d||d.ok===false){$('#cqRbig').attr('class','big bad').html(IC.x);$('#cqRt').text('The batch was not created');$('#cqRs').text('Nothing was saved. You can try again.');
    $('#cqRc').html('<ul class="cq-cl"><li class="bad" style="--i:0"><span class="mk">'+IC.x+'</span><div class="grow"><b>Error</b><p>'+esc(clean(d&& d.message))+'</p></div></li></ul>');
    $('#cqRf').html('<button type="button" class="gp-btn" data-r="close">Close</button><button type="button" class="gp-btn gp-btn--hot" data-r="create">'+IC.refresh+'<span>Try again</span></button>');step(3);open('cqRes');return;}
  S.created=d;step(4);
  $('#cqRbig').attr('class','big ok').html(IC.check);$('#cqRt').text('Batch '+(d.batchNo||'')+' created');
  $('#cqRs').text('Last step: open the batch and submit it to put the cheques in the client wallet.');
  $('#cqRc').html('<div class="cq-sum"><div><small>Batch</small><b>'+esc(d.batchNo||'')+'</b><span>'+esc(S.file.name)+'</span></div>'
    +'<div><small>Customer</small><b>'+esc(c.name||('Client '+(c.id||'')))+'</b><span>'+esc(c.id?'Client ID '+c.id:'')+'</span></div>'
    +'<div><small>Cheques loaded</small><b>'+esc(d.valid||0)+(d.errors?' <span style="display:inline;color:#B42318">· '+esc(d.errors)+' with errors</span>':'')+'</b><span>valid lines</span></div>'
    +'<div><small>Total amount</small><b>'+money(r.total)+'</b><span>'+esc($('#cqCur').val()||'')+'</span></div></div>'
    +'<ul class="cq-cl"><li class="ok" style="--i:0"><span class="mk">'+IC.check+'</span><div class="grow"><b>Batch created</b><p>The cheques are in the batch, not yet in the wallet.</p></div></li>'
    +'<li class="ok" style="--i:1"><span class="mk">'+IC.check+'</span><div class="grow"><b>Lines validated</b><p>'+esc(d.valid||0)+' valid line'+((d.valid===1)?'':'s')+(d.errors?', '+esc(d.errors)+' with errors':'')+'.</p></div></li></ul>');
  $('#cqRf').html('<button type="button" class="gp-btn" data-r="another">'+IC.refresh+'<span>Upload another file</span></button><button type="button" class="gp-btn gp-btn--hot" data-r="open">'+IC.wallet+'<span>Open the batch to submit</span></button>');
  open('cqRes');}
init();
});
