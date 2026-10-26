apex.jQuery(function(){
var IC={check:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 6 9 17l-5-5"/></svg>',x:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M18 6 6 18M6 6l12 12"/></svg>',wallet:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 7H5a2 2 0 0 1 0-4h13v4"/><path d="M3 5v14a2 2 0 0 0 2 2h15V7H5"/><path d="M16 14h.01"/></svg>',refresh:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M21 12a9 9 0 1 1-3-6.7L21 8"/><path d="M21 3v5h-5"/></svg>',trash:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 6h18M8 6V4h8v2M6 6l1 14h10l1-14"/></svg>',back:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M19 12H5M12 19l-7-7 7-7"/></svg>'};
var TO='<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg>';

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

var S={id:pageItem('P141_MOVEMENT_GROUP_ID'),d:null};
function load(){apex.server.process('CHQ_MOVE',{x01:S.id},{dataType:'json'}).then(function(d){
    if(!d||d.ok===false){$('#cmHead').html('<div class="cb-empty"><b>The movement could not be opened</b>'+esc(clean(d&& d.message))+'</div>');return;}S.d=d;render();},
  function(x){$('#cmHead').html('<div class="cb-empty"><b>The movement could not be opened</b>'+esc(clean(x&& x.responseText).substring(0,300))+'</div>');});}
function render(){var d=S.d,g=d.group,L=d.lines||[];
  $('#cmTitle').text(g.ref||('Movement #'+g.id));
  $('#cmHeroSt').html('<span class="cb-st SUBMITTED">'+esc(g.typeName||g.type)+'</span><small>'+esc(L.length)+' cheque'+(L.length===1?'':'s')+' · '+money(d.amount)+' '+esc(d.currency||'')+'</small>');
  $('#cmActs').html('<a class="gp-btn" href="'+esc(d.listUrl)+'">'+IC.back+'<span>All movements</span></a>'+(L.length?'':'<button type="button" class="gp-btn gp-btn--danger" data-a="DELETE">'+IC.trash+'<span>Delete</span></button>'));
  var h=[['Type',g.typeName||g.type,g.type],['To',g.toName||g.to||'-',g.to&& g.toName?g.to:''],['Date',g.date||'-',g.effective?'effective '+g.effective:''],['Deposit batch',g.depositRef||'-',g.depositRef?('#'+g.depositId):'']];
  $('#cmHd').html(h.map(function(x){return '<div><small>'+x[0]+'</small><b>'+esc(x[1])+'</b><span>'+esc(x[2]||'')+'</span></div>';}).join(''));
  $('#cmHead').html('<h2>Movement details</h2><p>The cheques of a movement cannot change; you can correct its reference, effective date and notes.</p>'
    +'<div class="cq-f"><label for="cmRef">Reference</label><input type="text" id="cmRef" maxlength="200" value="'+esc(g.ref||'')+'"></div>'
    +'<div class="cq-f"><label for="cmEff">Effective date</label><input type="date" id="cmEff" value="'+esc(g.effectiveIso||'')+'"></div>'
    +'<div class="cq-f"><label for="cmNotes">Notes</label><input type="text" id="cmNotes" maxlength="2000" value="'+esc(g.notes||'')+'"></div>'
    +'<div class="cd-save"><button type="button" class="gp-btn gp-btn--pri" data-a="SAVE">'+IC.check+'<span>Save</span></button></div>'
    +'<div class="cd-kv"><span>Created</span><b>'+esc((g.created||'')+(g.createdBy?' · '+g.createdBy:''))+'</b><span>Movement no</span><b>'+esc(g.id)+'</b></div>');
  $('#cmN').text(L.length+' cheque'+(L.length===1?'':'s'));
  if(!L.length){$('#cmLines').html('<div class="cb-empty"><b>No cheque in this movement</b>An empty movement can be deleted.</div>');return;}
  $('#cmLines').html('<div class="gp-tw"><table class="cb-t cm-t"><thead><tr><th>Cheque</th><th>Customer</th><th>Due date</th><th class="r">Amount</th><th>From &rarr; to</th><th>Status</th><th class="cb-hide">Handed over / received</th></tr></thead><tbody>'
    +L.map(function(l){return '<tr><td><a href="'+esc(l.url)+'">'+esc(l.ref||l.no)+'</a><small>'+esc(l.no||'')+(l.bank?' · '+esc(l.bank):'')+'</small></td>'
      +'<td><b>'+esc(l.customer||'-')+'</b><small>'+(l.customerId?'Client ID '+esc(l.customerId):'')+'</small></td>'
      +'<td>'+esc(l.due||'')+'</td><td class="r"><b>'+money(l.amount)+'</b><small>'+esc(l.currency||'')+'</small></td>'
      +'<td><div class="cm-path"><span>'+esc(l.fromName||l.from||'-')+'</span>'+TO+'<span class="to">'+esc(l.toName||l.to||'-')+'</span></div></td>'
      +'<td>'+(l.newStatus?'<span class="cd-ls VALID" style="background:#E6F3F4;color:#00636D">'+esc(l.newStatusName||l.newStatus)+'</span>':'')+(l.oldStatus?'<small>was '+esc(l.oldStatusName||l.oldStatus)+'</small>':'')
      +(l.reason?'<small style="color:#B42318">'+esc(l.reason)+'</small>':'')+(l.bankRef?'<small>Bank ref '+esc(l.bankRef)+'</small>':'')+'</td>'
      +'<td class="cb-hide">'+esc(l.handover||'-')+'<small>'+esc(l.receivedBy?('received by '+l.receivedBy+(l.receivedAt?' · '+l.receivedAt:'')):'')+'</small></td></tr>';}).join('')+'</tbody></table></div>');}
$(document).on('click','#cmActs [data-a],#cmHead [data-a]',function(){var a=$(this).data('a'),g=S.d.group;
  if(a==='SAVE'){run('CHQ_MOVE_ACT',{x01:'SAVE',x02:S.id,x03:$('#cmRef').val(),x04:$('#cmEff').val(),x05:$('#cmNotes').val()},'Saving the movement…',g.ref||'',['Saving reference, date and notes'],
      function(d){if(d.ok===false){popup('bad','Not saved','Nothing was changed.','<div class="cq-confirm danger">'+esc(clean(d.message))+'</div>',[['close','Close']]);return;}notify('ok','Movement saved','');load();});return;}
  if(a==='DELETE'){popup('bad','Delete this movement?','It has no cheques.','<div class="cq-confirm danger">The empty movement record is deleted. No cheque changes.</div>',[['close','Cancel'],['go','Delete','gp-btn--danger','trash']],function(k){if(k!=='go')return;
      run('CHQ_MOVE_ACT',{x01:'DELETE',x02:S.id},'Deleting the movement…','',['Removing the movement'],function(d){if(d.ok===false){popup('bad','Not deleted','','<div class="cq-confirm danger">'+esc(clean(d.message))+'</div>',[['close','Close']]);return;}
        notify('ok','Movement deleted','');setTimeout(function(){apex.navigation.redirect(S.d.listUrl);},900);});});}});
load();
});
