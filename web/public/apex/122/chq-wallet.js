apex.jQuery(function(){
var COLORS=["#00636D", "#E87722", "#5B5BD6", "#14734F", "#0EA5E9", "#B4570E", "#7C3AED", "#64748B"],ARROW='<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg>';
var S={rows:[],st:[],k:'ALL',q:'',due:'',loc:'',page:1,size:25};

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

function esc(v){return apex.util.escapeHTML(v==null?'':String(v));}
function money(n){n=+n||0;return n.toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2});}

function due(iso){if(!iso)return {d:null};var p=iso.split('-'),d=new Date(+p[0],+p[1]-1,+p[2]),t=new Date();t.setHours(0,0,0,0);return {d:Math.round((d-t)/86400000)};}
function dueBadge(r){if(!r.dueIso||r.terminal==='Y')return '';var n=due(r.dueIso).d;
  if(n<0)return '<span class="cw-due over">'+(-n)+' day'+(n===-1?'':'s')+' overdue</span>';
  if(n===0)return '<span class="cw-due soon">due today</span>';
  if(n<=30)return '<span class="cw-due soon">in '+n+' day'+(n===1?'':'s')+'</span>';return '<span class="cw-due later">in '+n+' days</span>';}

var SEM={COLLECTED:'#15803D',CLEARED:'#15803D',REJECTED:'#B42318',BOUNCED:'#B42318',RETURNED:'#B42318',WITHDRAWN:'#6B7280',CANCELLED:'#6B7280'};
function col(s){if(SEM[s])return SEM[s];var i=S.st.indexOf(s);return COLORS[(i<0?0:i)%COLORS.length];}
function shown(){var q=S.q.toLowerCase();return S.rows.filter(function(r){
  if(S.k==='OVER'){if(!(r.dueIso&& r.terminal!=='Y'&&due(r.dueIso).d<0))return false;}else if(S.k!=='ALL'&& r.status!==S.k)return false;
  if(S.loc&& r.loc!==S.loc)return false;
  if(S.due){if(!r.dueIso||r.terminal==='Y')return false;var n=due(r.dueIso).d;if(S.due==='OVER'?n>=0:(n<0||n>+S.due))return false;}
  return !q||[r.ref,r.no,r.customer,r.customerId,r.drawer,r.bank,r.bankName,r.batchNo].join(' ').toLowerCase().indexOf(q)>=0;});}
function tiles(){var c={},over=0;S.rows.forEach(function(r){c[r.status]=(c[r.status]||0)+1;if(r.dueIso&& r.terminal!=='Y'&&due(r.dueIso).d<0)over++;});
  var t=[['ALL','All cheques',S.rows.length,'#5F7B8A','In the wallet']];
  S.st.forEach(function(s){var r=S.rows.filter(function(y){return y.status===s;})[0]||{};t.push([s,r.statusName||s,c[s]||0,col(s),s]);});
  t.push(['OVER','Overdue',over,'#B42318','Past due, not final']);
  $('#cwKpis').css('grid-template-columns','repeat('+Math.min(t.length,6)+',minmax(0,1fr))').html(t.map(function(x){return '<button type="button" class="cb-k'+(S.k===x[0]?' on':'')+'" data-k="'+esc(x[0])+'" style="--c:'+x[3]+'"><span class="h"><i></i>'+esc(x[1])+'</span><b>'+x[2]+'</b><small>'+esc(x[4])+'</small></button>';}).join(''));}
function render(){var rows=shown(),tot={};rows.forEach(function(r){tot[r.currency||'']=(tot[r.currency||'']||0)+(+r.amount||0);});
  $('#cwCount').text(rows.length+' of '+S.rows.length+' cheques');$('#cwClear').toggleClass('show',!!(S.q||S.due||S.loc||S.k!=='ALL'));
  if(!rows.length){$('#cwList').html('<div class="cb-empty"><b>'+(S.rows.length?'No cheque matches':'No cheque yet')+'</b>'+(S.rows.length?'Change the search, the filters or the tile.':'Upload a cheque batch to fill the wallet.')+'</div>');return;}
  var shownRows=pageOf(rows,S);
  $('#cwList').html('<table class="cb-t"><thead><tr><th>Cheque</th><th>Customer</th><th>Bank</th><th>Due date</th><th class="r">Amount</th><th>Status</th><th>Location</th><th></th></tr></thead><tbody>'
    +shownRows.map(function(r){return '<tr data-url="'+esc(r.url)+'" tabindex="0"><td><span class="cb-no">'+esc(r.ref)+'</span><small>'+esc(r.no||'')+(r.batchNo?' · '+esc(r.batchNo):'')+'</small></td>'
      +'<td><b>'+esc(r.customer||'-')+'</b><small>'+(r.customerId?'Client ID '+esc(r.customerId):'')+'</small></td>'
      +'<td>'+esc(r.bank||'')+'<small>'+esc(r.bankName||'')+'</small></td>'
      +'<td style="white-space:nowrap">'+esc(r.due||'')+'<br>'+dueBadge(r)+'</td>'
      +'<td class="r"><b>'+money(r.amount)+'</b><small>'+esc(r.currency||'')+'</small></td>'
      +'<td><span class="cw-st" style="--c:'+col(r.status)+'">'+esc(r.statusName||r.status)+'</span></td>'
      +'<td>'+esc(r.locName||r.loc||'-')+'<small>'+esc(r.custodian||'')+'</small></td>'
      +'<td><span class="cb-open">'+ARROW+'</span></td></tr>';}).join('')+'</tbody></table>'
    +'<div class="cw-foot"><span>Total of the '+rows.length+' cheque'+(rows.length===1?'':'s')+' found</span><span>'
    +Object.keys(tot).map(function(k){return '<b>'+money(tot[k])+'</b> '+esc(k);}).join(' · ')+'</span></div>'+pager(rows.length,S.page,S.size));}
bindPager('#cwList',S,render);
$('#cwKpis').on('click','.cb-k',function(){S.k=String($(this).data('k'));S.page=1;tiles();render();});
$('#cwQ').on('input',function(){S.q=$.trim(this.value);S.page=1;render();});
$('#cwDue').on('click','.cx-chip',function(){S.due=String($(this).data('v'));S.page=1;$('#cwDue .cx-chip').removeClass('on');$(this).addClass('on');render();});
$('#cwLoc').on('change',function(){S.loc=this.value;S.page=1;render();});
$('#cwClear').on('click',function(){S.q='';S.due='';S.loc='';S.k='ALL';S.page=1;$('#cwQ').val('');$('#cwLoc').val('');$('#cwDue .cx-chip').removeClass('on').first().addClass('on');tiles();render();});
$('#cwList').on('click','tbody tr',function(){var u=$(this).data('url');if(u)apex.navigation.redirect(u);})
  .on('keydown','tbody tr',function(e){if(e.key==='Enter'){var u=$(this).data('url');if(u)apex.navigation.redirect(u);}});
$('#cwCsv').on('click',function(){var rows=shown();var q=function(v){v=v==null?'':String(v);return /[",\n;]/.test(v)?'"'+v.replace(/"/g,'""')+'"':v;};
  var csv=[['Cheque ref','Cheque no','Customer','Client ID','Drawer','Bank','Due date','Amount','Currency','Status','Location','Custodian','Batch'].join(',')].concat(rows.map(function(r){
    return [r.ref,r.no,r.customer,r.customerId,r.drawer,r.bank,r.dueIso,r.amount,r.currency,r.statusName||r.status,r.locName||r.loc,r.custodian,r.batchNo].map(q).join(',');})).join('\r\n');
  var b=new Blob(['﻿'+csv],{type:'text/csv;charset=utf-8'}),a=document.createElement('a');a.href=URL.createObjectURL(b);a.download='cheques-wallet.csv';document.body.appendChild(a);a.click();
  setTimeout(function(){URL.revokeObjectURL(a.href);a.remove();},500);});
apex.server.process('CHQ_WALLET',{},{dataType:'json'}).then(function(d){S.rows=(d&& d.rows)||[];$('#cwUp').attr('href',d.uploadUrl||'#');
  S.st=(d.statuses||[]).filter(function(s){return S.rows.some(function(r){return r.status===s;});});
  S.rows.forEach(function(r){if(S.st.indexOf(r.status)<0)S.st.push(r.status);});
  $('#cwLoc').append((d.locations||[]).map(function(l){return '<option value="'+esc(l.code)+'">'+esc(l.name)+'</option>';}).join(''));
  var h=(location.hash||'').replace('#','');if(h)S.k=h;tiles();render();},
  function(x){$('#cwList').html('<div class="cb-empty"><b>The cheques could not be loaded</b>'+esc(String(x&& x.responseText||'').substring(0,300))+'</div>');});
});
