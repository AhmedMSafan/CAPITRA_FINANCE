apex.jQuery(function(){
var IC={check:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 6 9 17l-5-5"/></svg>',x:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M18 6 6 18M6 6l12 12"/></svg>',wallet:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 7H5a2 2 0 0 1 0-4h13v4"/><path d="M3 5v14a2 2 0 0 0 2 2h15V7H5"/><path d="M16 14h.01"/></svg>',refresh:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M21 12a9 9 0 1 1-3-6.7L21 8"/><path d="M21 3v5h-5"/></svg>',trash:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 6h18M8 6V4h8v2M6 6l1 14h10l1-14"/></svg>',back:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M19 12H5M12 19l-7-7 7-7"/></svg>',move:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M17 1l4 4-4 4"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/><path d="M7 23l-4-4 4-4"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/></svg>',list:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M8 6h13M8 12h13M8 18h13"/><path d="M3 6h.01M3 12h.01M3 18h.01"/></svg>',plus:'<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 5v14M5 12h14"/></svg>'};
IC.ask=IC.move;
var TO='<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg>',ARROW='<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg>',COLORS=["#00636D", "#E87722", "#5B5BD6", "#14734F", "#0EA5E9", "#B4570E", "#7C3AED", "#64748B"],ICHQ='<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="2" y="6" width="20" height="12" rx="2"/><path d="M6 10h7M6 14h4M15 14h3"/></svg>';

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

function pager(total,page,size){var pages=Math.max(1,Math.ceil(total/size));page=Math.min(Math.max(1,page),pages);
  var a=total?(page-1)*size+1:0,b=Math.min(total,page*size),n=[],i;
  for(i=1;i<=pages;i++){if(i===1||i===pages||Math.abs(i-page)<=1)n.push(i);else if(n[n.length-1]!=='…')n.push('…');}
  return '<div class="cx-pg"><span class="cx-tot">Showing <b>'+a+'–'+b+'</b> of <b>'+total+'</b></span><div class="cx-pgr">'
    +'<label>Rows <select data-pg="size">'+[10,25,50,100].map(function(s){return '<option'+(s===size?' selected':'')+'>'+s+'</option>';}).join('')+'</select></label>'
    +'<button type="button" class="cx-pb" data-pg="'+(page-1)+'"'+(page<=1?' disabled':'')+' aria-label="Previous page">&lsaquo;</button>'
    +n.map(function(x){return x==='…'?'<span class="dots">…</span>':'<button type="button" class="cx-pb'+(x===page?' on':'')+'" data-pg="'+x+'"'+(x===page?' aria-current="page"':'')+'>'+x+'</button>';}).join('')
    +'<button type="button" class="cx-pb" data-pg="'+(page+1)+'"'+(page>=pages?' disabled':'')+' aria-label="Next page">&rsaquo;</button></div></div>';}
function bindPager(sel,st,draw){$(sel).on('click','.cx-pb[data-pg]',function(){if(this.disabled)return;st.page=+$(this).data('pg');draw();$(sel)[0].scrollIntoView({block:'nearest'});})
  .on('change','select[data-pg=size]',function(){st.size=+this.value;st.page=1;draw();});}
function pageOf(rows,st){var pages=Math.max(1,Math.ceil(rows.length/st.size));if(st.page>pages)st.page=pages;if(st.page<1)st.page=1;return rows.slice((st.page-1)*st.size,st.page*st.size);}

var S={rows:[],acts:[],banks:[],accts:[],k:'ALL',q:'',loc:'',st:'',page:1,size:25},H={rows:null,q:'',t:'',page:1,size:25};
var KP=[['ALL','Total cheques','#5F7B8A',function(){return true;}],
  ['OPS','With operations','#E87722',function(r){return r.loc==='OPERATIONS'||r.loc==='COMPANY_VAULT';}],
  ['TRS','With treasury','#00636D',function(r){return r.loc==='TREASURY';}],
  ['DEP','Deposited','#5B5BD6',function(r){return r.status==='DEPOSITED';}]];
function kf(k){for(var i=0;i<KP.length;i++)if(KP[i][0]===k)return KP[i][3];return KP[0][3];}
function locB(code,name){return '<span class="cx-loc '+esc(code||'')+'">'+esc(name||code||'-')+'</span>';}
var SEM={COLLECTED:'#15803D',REJECTED:'#B42318',RECEIVED:'#B45309',DEPOSITED:'#3730A3'};
function stB(r){return '<span class="cw-st" style="--c:'+(SEM[r.status]||'#4B5F69')+'">'+esc(r.statusName||r.status)+'</span>';}
function shown(){var q=S.q.toLowerCase(),f=kf(S.k);return S.rows.filter(function(r){if(!f(r))return false;if(S.loc&& r.loc!==S.loc)return false;if(S.st&& r.status!==S.st)return false;
  return !q||[r.no,r.ref,r.client,r.clientId,r.bank,r.bankCode,r.heldBy,r.locName,r.statusName].join(' ').toLowerCase().indexOf(q)>=0;});}
function tiles(){$('#cxKpis').html(KP.map(function(x){var n=S.rows.filter(x[3]).length;
  return '<button type="button" class="cb-k'+(S.k===x[0]?' on':'')+'" data-k="'+x[0]+'" style="--c:'+x[2]+'"><span class="h"><i></i>'+esc(x[1])+'</span><b>'+n+'</b></button>';}).join(''));}
function render(){var rows=shown();$('#cxN1').text(S.rows.length);$('#cxCount').text(rows.length+' of '+S.rows.length+' cheques');
  $('#cxClear').toggleClass('show',!!(S.q||S.loc||S.st||S.k!=='ALL'));
  if(!rows.length){$('#cxList').html('<div class="cb-empty"><b>'+(S.rows.length?'No cheque matches':'No open cheque')+'</b>'+(S.rows.length?'Change the search, the filters or the tile.':'Open cheques appear here once a batch is submitted.')+'</div>');return;}
  $('#cxList').html('<table class="cb-t"><thead><tr><th>Cheque no</th><th>Client</th><th class="cb-hide">Bank</th><th class="r">Amount</th><th>Location</th><th class="cb-hide">Held by</th><th>Since</th><th>Status</th><th></th></tr></thead><tbody>'
    +pageOf(rows,S).map(function(r){return '<tr data-url="'+esc(r.url)+'" data-id="'+esc(r.id)+'" tabindex="0"><td><span class="cb-no">'+esc(r.no||'')+'</span><small>'+esc(r.ref||'')+'</small></td>'
      +'<td><b>'+esc(r.client||'-')+'</b><small>'+(r.clientId?'Client ID '+esc(r.clientId):'')+'</small></td>'
      +'<td class="cb-hide">'+esc(r.bank||r.bankCode||'')+'<small>'+(r.due?'due '+esc(r.due):'')+'</small></td>'
      +'<td class="r"><b>'+money(r.amount)+'</b><small>'+esc(r.currency||'')+'</small></td>'
      +'<td>'+locB(r.loc,r.locName)+'</td><td class="cb-hide">'+esc(r.heldBy||'-')+'</td><td style="white-space:nowrap">'+esc(r.since||'-')+'</td><td>'+stB(r)+'</td>'
      +'<td><button type="button" class="cx-mv" data-mv="'+esc(r.id)+'">'+IC.move+'Move</button></td></tr>';}).join('')+'</tbody></table>'+pager(rows.length,S.page,S.size));}
bindPager('#cxList',S,render);
// ---- history tab (former page 140)
var HC=[];function hcol(t){var i=HC.indexOf(t);if(i<0){HC.push(t);i=HC.length-1;}return COLORS[i%COLORS.length];}
function hshown(){var q=H.q.toLowerCase();return (H.rows||[]).filter(function(r){if(H.t&& r.type!==H.t)return false;
  return !q||[r.ref,r.notes,r.typeName,r.to,r.toName,r.createdBy,r.depositRef].join(' ').toLowerCase().indexOf(q)>=0;});}
function hrender(){var rows=hshown();$('#cxN2').text(H.rows.length);$('#cxHCount').text(rows.length+' of '+H.rows.length+' movements');
  if(!rows.length){$('#cxHist').html('<div class="cb-empty"><b>'+(H.rows.length?'No movement matches':'No movement yet')+'</b>'+(H.rows.length?'Change the search or the type.':'Movements appear here when cheques are moved.')+'</div>');return;}
  $('#cxHist').html('<table class="cb-t cm-t"><thead><tr><th>Movement</th><th>Type</th><th>To</th><th class="r">Cheques</th><th class="r">Amount</th><th>Date</th><th class="cb-hide">By</th><th></th></tr></thead><tbody>'
    +pageOf(rows,H).map(function(r){return '<tr data-url="'+esc(r.url)+'" tabindex="0"><td><span class="cb-no">'+esc(r.ref||('#'+r.id))+'</span>'+(r.depositRef?'<small>Deposit batch '+esc(r.depositRef)+'</small>':(r.notes?'<small>'+esc(r.notes)+'</small>':''))+'</td>'
      +'<td><span class="cm-type" style="--c:'+hcol(r.type)+'">'+esc(r.typeName||r.type)+'</span></td>'
      +'<td>'+locB(r.to,r.toName)+'</td><td class="r"><b>'+esc(r.cheques)+'</b></td><td class="r"><b>'+money(r.amount)+'</b><small>'+esc(r.currency||'')+'</small></td>'
      +'<td style="white-space:nowrap">'+esc(r.date||'')+(r.effective&& r.effective!==r.date?'<small>effective '+esc(r.effective)+'</small>':'')+'</td>'
      +'<td class="cb-hide">'+esc(r.createdBy||'')+'<small>'+esc(r.created||'')+'</small></td><td><span class="cb-open">'+ARROW+'</span></td></tr>';}).join('')+'</tbody></table>'
    +(H.more?'<div class="cb-more">The latest 1000 movements are listed.</div>':'')+pager(rows.length,H.page,H.size));}
bindPager('#cxHist',H,hrender);
function loadHist(){apex.server.process('CHQ_MOVES',{},{dataType:'json'}).then(function(d){H.rows=(d&& d.rows)||[];H.more=d&& d.more;
    var t=[];H.rows.forEach(function(r){if(t.indexOf(r.type)<0){t.push(r.type);$('#cxHT').append('<option value="'+esc(r.type)+'">'+esc(r.typeName||r.type)+'</option>');}});hrender();},
  function(x){$('#cxHist').html('<div class="cb-empty"><b>The movements could not be loaded</b>'+esc(clean(x&& x.responseText).substring(0,300))+'</div>');});}
function tab(t){$('.cx-tab').removeClass('on').attr('aria-selected','false');$('.cx-tab[data-t='+t+']').addClass('on').attr('aria-selected','true');
  $('#cxPaneCHQ').prop('hidden',t!=='CHQ');$('#cxPaneHIST').prop('hidden',t!=='HIST');if(t==='HIST'&& H.rows===null){H.rows=[];loadHist();}}
$('.cx-tab').on('click',function(){tab($(this).data('t'));});
$('#cxHQ').on('input',function(){H.q=$.trim(this.value);H.page=1;hrender();});
$('#cxHT').on('change',function(){H.t=this.value;H.page=1;hrender();});
$('#cxHist').on('click','tbody tr',function(){var u=$(this).data('url');if(u)apex.navigation.redirect(u);})
  .on('keydown','tbody tr',function(e){if(e.key==='Enter'){var u=$(this).data('url');if(u)apex.navigation.redirect(u);}});
// ---- cheques tab events
$('#cxKpis').on('click','.cb-k',function(){S.k=String($(this).data('k'));S.page=1;tab('CHQ');tiles();render();});
$('#cxQ').on('input',function(){S.q=$.trim(this.value);S.page=1;render();});
$('#cxLoc').on('change',function(){S.loc=this.value;S.page=1;render();});
$('#cxSt').on('change',function(){S.st=this.value;S.page=1;render();});
$('#cxClear').on('click',function(){S.q=S.loc=S.st='';S.k='ALL';S.page=1;$('#cxQ').val('');$('#cxLoc').val('');$('#cxSt').val('');tiles();render();});
$('#cxList').on('click','.cx-mv',function(e){e.stopPropagation();moveForm($(this).data('mv'));})
  .on('click','tbody tr',function(){var u=$(this).data('url');if(u)apex.navigation.redirect(u);})
  .on('keydown','tbody tr',function(e){if(e.key==='Enter'&& e.target===this){var u=$(this).data('url');if(u)apex.navigation.redirect(u);}});
$('#cxNew').on('click',function(){moveForm(null);});
$('#cxCsv').on('click',function(){var hist=!$('#cxPaneHIST').prop('hidden'),q=function(v){v=v==null?'':String(v);return /[",\n;]/.test(v)?'"'+v.replace(/"/g,'""')+'"':v;},csv;
  if(hist)csv=[['Reference','Type','To location','Cheques','Amount','Currency','Date','Effective','Deposit batch','Created by','Created','Notes'].join(',')].concat(hshown().map(function(r){
    return [r.ref,r.typeName||r.type,r.toName||r.to,r.cheques,r.amount,r.currency,r.date,r.effective,r.depositRef,r.createdBy,r.created,r.notes].map(q).join(',');}));
  else csv=[['Cheque no','Cheque ref','Client','Client ID','Bank','Amount','Currency','Location','Held by','Since','Status','Due date'].join(',')].concat(shown().map(function(r){
    return [r.no,r.ref,r.client,r.clientId,r.bank,r.amount,r.currency,r.locName||r.loc,r.heldBy,r.since,r.statusName||r.status,r.due].map(q).join(',');}));
  var b=new Blob(['﻿'+csv.join('\r\n')],{type:'text/csv;charset=utf-8'}),a=document.createElement('a');a.href=URL.createObjectURL(b);a.download=hist?'cheque-movement-history.csv':'cheques-to-move.csv';
  document.body.appendChild(a);a.click();setTimeout(function(){URL.revokeObjectURL(a.href);a.remove();},500);});
// ---- the movement popup (page 4)
function byId(id){for(var i=0;i<S.rows.length;i++)if(String(S.rows[i].id)===String(id))return S.rows[i];return null;}
function act(code){for(var i=0;i<S.acts.length;i++)if(S.acts[i].code===code)return S.acts[i];return null;}
function chqCard(r,fixed){return '<div class="cx-chq"><span class="ic">'+ICHQ+'</span><div class="tx"><b>'+esc(r.no||r.ref)+(fixed?'':'<button type="button" class="chg" id="cxChg">Change</button>')+'</b><small>'+esc(r.client||'')+(r.bank?' · '+esc(r.bank):'')+'</small>'
  +'<small>Now: '+esc(r.locName||r.loc||'-')+' · '+esc(r.statusName||r.status||'-')+(r.heldBy?' · held by '+esc(r.heldBy):'')+'</small></div><div class="am">'+money(r.amount)+'<small>'+esc(r.currency||'')+'</small></div></div>';}
function pickList(q){q=(q||'').toLowerCase();var L=S.rows.filter(function(r){return !q||[r.no,r.ref,r.client,r.clientId,r.bank].join(' ').toLowerCase().indexOf(q)>=0;})
    .sort(function(a,b){return (a.dueIso||'')<(b.dueIso||'')?-1:(a.dueIso||'')>(b.dueIso||'')?1:String(a.no).localeCompare(String(b.no));});
  return L.length?L.slice(0,80).map(function(r){return '<li data-id="'+esc(r.id)+'" tabindex="0"><b>'+esc(r.no||r.ref)+'</b><span>'+esc(r.client||'')+'</span><em>'+money(r.amount)+'</em></li>';}).join('')+(L.length>80?'<li class="none">'+(L.length-80)+' more: type to narrow the list</li>':''):'<li class="none">No open cheque matches</li>';}
function ruleCard(){var a=act($('#cxAct').val()),r=byId(M.id);if(!a){$('#cxRule').html('<div class="cx-hint">Choose an action to see where it moves the cheque.</div>');return;}
  $('#cxRule').html('<div class="route">'+(r?locB(r.loc,r.locName):'')+TO+locB(a.to,a.toName||a.to||'not set')+'</div><dl>'
    +'<dt>Event</dt><dd>'+esc(a.eventName||a.event||'-')+'</dd>'
    +(a.fromStatus?'<dt>Allowed from status</dt><dd>'+esc(a.fromStatus)+'</dd>':'')
    +(a.fromLoc?'<dt>Allowed from location</dt><dd>'+esc(a.fromLoc)+'</dd>':'')
    +'</dl>'+(a.active!=='Y'?'<div class="cx-hint" style="color:#B42318">This action is not active in the action catalog.</div>':''));
  var dep=a.code==='DEPOSIT_TO_BANK';$('#cxBankRow').prop('hidden',!dep);}
var M={id:null};
function accts(){var b=$('#cxBank').val();$('#cxAcct').html('<option value=""></option>'+S.accts.filter(function(x){return String(x[2])===String(b);}).map(function(x){return '<option value="'+esc(x[0])+'">'+esc(x[1])+'</option>';}).join(''));}
function moveForm(id){M={id:id||null,fixed:!!id};
  var acts='<option value="">Choose an action</option>'+S.acts.map(function(a){return '<option value="'+esc(a.code)+'"'+(a.active!=='Y'?' disabled':'')+'>'+esc(a.name||a.code)+(a.active!=='Y'?' (inactive)':'')+'</option>';}).join('');
  var body='<div class="cx-form"><div class="cq-f"><label>Cheque</label><div id="cxChqBox"></div></div>'
    +'<div class="cq-f"><label for="cxAct">Action</label><select id="cxAct">'+acts+'</select></div><div class="cx-rule" id="cxRule"></div>'
    +'<div class="cx-two" id="cxBankRow" hidden><div class="cq-f"><label for="cxBank">Bank</label><select id="cxBank"><option value=""></option>'+S.banks.map(function(x){return '<option value="'+esc(x[0])+'">'+esc(x[1])+'</option>';}).join('')+'</select></div>'
    +'<div class="cq-f"><label for="cxAcct">Account</label><select id="cxAcct"><option value=""></option></select></div></div>'
    +(S.listErr?'<div class="cx-hint" style="color:#B42318">'+esc(S.listErr)+'</div>':'')+'</div>';
  popup('ask',id?'Move cheque':'Log movement','Apply an action from the action catalog to a cheque.',body,[['close','Cancel'],['go','Execute movement','gp-btn--pri','move']],function(k){if(k==='go')execute();});
  $('#cqRbig').attr('class','big ok').html(IC.move);chqBox();ruleCard();
  $('#cxAct').on('change',function(){$('#cqRerr').removeClass('show');ruleCard();});$('#cxBank').on('change',accts);}
function chqBox(){var r=byId(M.id);
  if(r){$('#cxChqBox').html(chqCard(r,M.fixed));$('#cxChg').on('click',function(){M.id=null;chqBox();ruleCard();});return;}
  $('#cxChqBox').html('<div class="cx-pick"><input type="search" id="cxPq" placeholder="Type a cheque no, client or bank" aria-label="Find a cheque"><ul id="cxPl">'+pickList('')+'</ul></div>');
  $('#cxPq').on('input',function(){$('#cxPl').html(pickList(this.value));}).trigger('focus');
  $('#cxPl').on('click keydown','li[data-id]',function(e){if(e.type==='keydown'&& e.key!=='Enter')return;M.id=$(this).data('id');$('#cqRerr').removeClass('show');chqBox();ruleCard();});}
function execute(){var a=$('#cxAct').val(),b=$('#cxBank').val(),c=$('#cxAcct').val(),r=byId(M.id),err='';
  if(!r)err='Please choose the cheque.';else if(!a)err='Please choose the action.';
  else if(a==='DEPOSIT_TO_BANK'&&!b)err='Please select a bank.';else if(a==='DEPOSIT_TO_BANK'&&!c)err='Please select a bank account.';
  if(err){$('#cqRerr').text(err).addClass('show');return;}
  var A=act(a);
  run('CHQ_MV_RUN',{x01:r.id,x02:a,x03:a==='DEPOSIT_TO_BANK'?b:'',x04:a==='DEPOSIT_TO_BANK'?c:''},'Executing the movement…',(r.no||r.ref)+' · '+(A&& A.name||a),
    ['Checking the action rules','Moving the cheque','Recording the movement'],function(d){
    if(d.ok===false){popup('bad','Movement not executed','Nothing was changed.','<div class="cq-confirm danger">'+esc(clean(d.message))+'</div>',[['close','Close'],['again','Try again','gp-btn--pri','move']],function(k){if(k==='again')moveForm(r.id);});return;}
    var body='<div class="cx-res">'+chqCard({no:r.no,client:r.client,bank:r.bank,amount:r.amount,currency:r.currency,loc:d.loc,locName:d.locName,status:d.status,statusName:d.statusName,heldBy:d.heldBy},true)
      +'<div class="route">'+locB(r.loc,r.locName)+TO+locB(d.loc,d.locName)+'</div>'
      +(d.count===0?'<div class="cq-confirm danger">The action reported that no cheque was moved.</div>':'')+'</div>';
    var btn=[['close','Close']];if(d.moveUrl)btn.push(['open','Open the movement','','list']);btn.push(['more','Move another cheque','gp-btn--pri','move']);
    popup('ok','Movement executed successfully',(A&& A.name||a)+(d.groupId?' · movement #'+d.groupId:''),body,btn,function(k){if(k==='open')apex.navigation.redirect(d.moveUrl);if(k==='more')moveForm(null);});
    notify('ok','Movement executed',(r.no||r.ref)+' → '+(d.locName||d.loc||''));H.rows=null;load();});}
function load(){apex.server.process('CHQ_MV_LIST',{},{dataType:'json'}).then(function(d){if(!d||d.ok===false){$('#cxList').html('<div class="cb-empty"><b>The cheques could not be loaded</b>'+esc(clean(d&& d.message))+'</div>');return;}
    S.rows=d.rows||[];S.acts=d.actions||[];S.banks=d.banks||[];S.accts=d.accounts||[];S.listErr=d.listError;
    var l=$('#cxLoc').val(),s=$('#cxSt').val(),L={},T={};S.rows.forEach(function(r){if(r.loc)L[r.loc]=r.locName||r.loc;if(r.status)T[r.status]=r.statusName||r.status;});
    $('#cxLoc').html('<option value="">All locations</option>'+Object.keys(L).map(function(k){return '<option value="'+esc(k)+'">'+esc(L[k])+'</option>';}).join('')).val(l);
    $('#cxSt').html('<option value="">All statuses</option>'+Object.keys(T).map(function(k){return '<option value="'+esc(k)+'">'+esc(T[k])+'</option>';}).join('')).val(s);
    tiles();render();if(!$('#cxPaneHIST').prop('hidden')){H.rows=[];loadHist();}},
  function(x){$('#cxList').html('<div class="cb-empty"><b>The cheques could not be loaded</b>'+esc(clean(x&& x.responseText).substring(0,300))+'</div>');});}
load();
if((location.hash||'').replace('#','').toUpperCase()==='HIST')tab('HIST');
});
