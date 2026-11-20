apex.jQuery(function(){
var S={rows:[],k:'ALL',q:''};
function esc(v){return apex.util.escapeHTML(v==null?'':String(v));}
function money(n){n=+n||0;return n.toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2});}
function lbl(v){v=String(v||'').replace(/_/g,' ').toLowerCase();return v.replace(/\b\w/g,function(c){return c.toUpperCase();});}
var ARROW='<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg>';
function group(r){var s=String(r.status||'').toUpperCase();
  if(s==='SUBMITTED')return 'SUBMITTED';if(s==='HAS_ERRORS')return 'ERRORS';if(s==='VALIDATED')return 'READY';return 'OPEN';}
function stName(s){s=String(s||'').toUpperCase();return {VALIDATED:'Ready to submit',HAS_ERRORS:'With errors',SUBMITTED:'Submitted',LOADED:'Loaded',DRAFT:'Draft'}[s]||lbl(s);}
function shown(){var q=S.q.toLowerCase();return S.rows.filter(function(r){
  if(S.k!=='ALL'&&group(r)!==S.k)return false;
  if(!q)return true;return [r.no,r.customerId,r.customer,r.customerAr,r.file,r.notes].join(' ').toLowerCase().indexOf(q)>=0;});}
function counts(){var c={ALL:S.rows.length,READY:0,ERRORS:0,OPEN:0,SUBMITTED:0};S.rows.forEach(function(r){c[group(r)]++;});
  Object.keys(c).forEach(function(k){$('#cbN'+k).text(c[k]);});}
function render(){var rows=shown(),h='';
  $('#cbCount').text(rows.length+' of '+S.rows.length+' batch'+(S.rows.length===1?'':'es'));
  if(!rows.length){$('#cbList').html('<div class="cb-empty"><b>'+(S.rows.length?'No batch matches':'No batch yet')+'</b>'+(S.rows.length?'Change the search or choose another tile.':'Click Upload cheques to load the first Excel file.')+'</div>');return;}
  h='<table class="cb-t"><thead><tr><th>Batch</th><th>Customer</th><th>Cheques</th><th class="r">Amount</th><th>Status</th><th class="cb-hide">Created</th><th class="cb-hide">Submitted</th><th></th></tr></thead><tbody>';
  h+=rows.map(function(r){var t=+r.total||0,v=+r.valid||0,e=+r.errors||0,s=+r.submitted||0,w=function(n){return t?(n/t*100).toFixed(1):0;};
    return '<tr data-url="'+esc(r.url)+'" tabindex="0"><td><span class="cb-no">'+esc(r.no)+'</span><small title="'+esc(r.file)+'">'+esc(r.file||'')+'</small></td>'
      +'<td><b>'+esc(r.customer||'-')+'</b><small>'+(r.customerId?'Client ID '+esc(r.customerId):'')+'</small></td>'
      +'<td><div class="cb-lines"><div class="m"><i style="width:'+w(s)+'%;background:#00636D"></i><i style="width:'+w(Math.max(0,v))+'%;background:#2BB673"></i><i style="width:'+w(e)+'%;background:#D9534F"></i></div>'
      +'<span>'+t+' line'+(t===1?'':'s')+(s?' · '+s+' submitted':'')+(v?' · '+v+' valid':'')+(e?' · <b style="color:#B42318">'+e+' with errors</b>':'')+'</span></div></td>'
      +'<td class="r"><b>'+money(r.amount)+'</b><small>'+esc(r.currency||'')+'</small></td>'
      +'<td><span class="cb-st '+esc(String(r.status||'').toUpperCase())+'">'+esc(stName(r.status))+'</span></td>'
      +'<td class="cb-hide">'+esc(r.created||'')+'<small>'+esc(r.createdBy||'')+'</small></td>'
      +'<td class="cb-hide">'+esc(r.submittedAt||'-')+'<small>'+esc(r.submittedBy||'')+'</small></td>'
      +'<td><span class="cb-open">'+ARROW+'</span></td></tr>';}).join('')+'</tbody></table>';
  if(S.more)h+='<div class="cb-more">The latest 1000 batches are listed. Use the search to find an older one.</div>';
  $('#cbList').html(h);}
$('#cbKpis').on('click','.cb-k',function(){S.k=$(this).data('k');$('#cbKpis .cb-k').removeClass('on');$(this).addClass('on');render();
  try{history.replaceState(null,'','#'+S.k);}catch(e){}});
$('#cbQ').on('input',function(){S.q=$.trim(this.value);render();});
$('#cbList').on('click','tbody tr',function(){var u=$(this).data('url');if(u)apex.navigation.redirect(u);})
  .on('keydown','tbody tr',function(e){if(e.key==='Enter'){var u=$(this).data('url');if(u)apex.navigation.redirect(u);}});
$('#cbCsv').on('click',function(){var rows=shown();
  var head=['Batch no','File','Client ID','Customer','Lines','Valid','Errors','Submitted lines','Amount','Currency','Status','Created','Created by','Submitted','Submitted by'];
  var q=function(v){v=v==null?'':String(v);return /[",\n;]/.test(v)?'"'+v.replace(/"/g,'""')+'"':v;};
  var csv=[head.join(',')].concat(rows.map(function(r){return [r.no,r.file,r.customerId,r.customer,r.total,r.valid,r.errors,r.submitted,r.amount,r.currency,stName(r.status),r.created,r.createdBy,r.submittedAt,r.submittedBy].map(q).join(',');})).join('\r\n');
  var b=new Blob(['﻿'+csv],{type:'text/csv;charset=utf-8'}),a=document.createElement('a');a.href=URL.createObjectURL(b);a.download='cheque-batches.csv';document.body.appendChild(a);a.click();
  setTimeout(function(){URL.revokeObjectURL(a.href);a.remove();},500);});
apex.server.process('CHQ_BATCHES',{},{dataType:'json'}).then(function(d){
  S.rows=(d&& d.rows)||[];S.more=d&& d.more;$('#cbNew').attr('href',d.uploadUrl||'#');
  var h=(location.hash||'').replace('#','');if(h&&$('#cbKpis .cb-k[data-k="'+h+'"]').length){S.k=h;$('#cbKpis .cb-k').removeClass('on');$('#cbKpis .cb-k[data-k="'+h+'"]').addClass('on');}
  counts();render();},function(x){$('#cbList').html('<div class="cb-empty"><b>The batches could not be loaded</b>'+esc(String(x&& x.responseText||'').substring(0,300))+'</div>');});
});
