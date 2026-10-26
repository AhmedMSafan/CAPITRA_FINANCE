apex.jQuery(function(){
(function($){
'use strict';
var IC={search:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="11" cy="11" r="7"/><path d="m21 21-4.3-4.3"/></svg>'};
var S={d:null,init:false};
function esc(v){return apex.util.escapeHTML(v==null?'':String(v));}
function money(n){return (+n||0).toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2});}
function drcr(n){n=+n||0;return money(Math.abs(n))+(n>0?' Dr':(n<0?' Cr':''));}
function fd(d){if(!d)return '-';var p=String(d).split('-');var m=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];return p.length<3?d:p[2]+' '+m[+p[1]-1]+' '+p[0];}
function clean(m){return String(m||'').replace(/^ORA-\d+:\s*/,'').replace(/\nORA-[\s\S]*$/,'');}
function err(m){apex.message.clearErrors();apex.message.showErrors([{type:'error',location:'page',message:clean(m),unsafe:false}]);}
function call(name,data,ok){apex.server.process(name,data,{dataType:'json'}).then(function(d){if(d&&d.ok===false){err(d.message||'The request failed.');return;}ok(d||{});},function(x){err((x&&x.responseText)||'The request failed.');});}
function opt(v,t,sel){return '<option value="'+esc(v)+'"'+(String(v)===String(sel)?' selected':'')+'>'+esc(t)+'</option>';}
function filters(){var f={};$('#alF [data-f]').each(function(){var v=$.trim($(this).val());if(v)f[$(this).data('f')]=v;});
  var sg={};$('#alF [data-seg]').each(function(){var v=$.trim($(this).val()).toUpperCase();if(v)sg[$(this).data('seg')]=v;});f.segments=sg;return f;}
function renderFilters(){var d=S.d,cur=d.currentPeriod||'';
  var acc=(d.segments||[]).filter(function(s){return s.code==='NATURAL';})[0];
  var others=(d.segments||[]).filter(function(s){if(s.code==='NATURAL')return false;var n=((d.values||{})[s.code]||[]).filter(function(v){return v[2]==='Y';}).length;return !(s.defMode==='FIXED'&&s.def)&&n!==1;});
  var dl=function(s){return '<datalist id="alDl'+s.num+'">'+((d.values||{})[s.code]||[]).map(function(v){return '<option value="'+esc(v[0])+'" label="'+esc(v[1])+'"></option>';}).join('')+'</datalist>';};
  var h='<div class="gs-g gs-g3">'
   +(acc?'<div class="span2"><label for="alAcc">Account (code, or the first digits for a group)</label><input id="alAcc" data-seg="NATURAL" list="alDl'+acc.num+'" class="code" placeholder="e.g. 110101 or 1101">'+dl(acc)+'</div>':'')
   +'<div><label for="alFrom">From period</label><select id="alFrom" data-f="from">'+(d.periods||[]).map(function(p){return opt(p,p,cur);}).join('')+'</select></div>'
   +'<div><label for="alTo">To period</label><select id="alTo" data-f="to">'+(d.periods||[]).map(function(p){return opt(p,p,cur);}).join('')+'</select></div>'
   +'<div class="span2"><label for="alQ">Journal or TRAN_SET_ID (optional)</label><input type="search" id="alQ" data-f="q" placeholder="Journal no, name or TRAN_SET_ID"></div></div>';
  if(others.length)h+='<div class="gs-more" id="alMore"><div class="gs-segs" style="border:0;padding:0;margin:0">'+others.map(function(s){return '<div><label for="alSeg'+s.num+'">'+esc(s.name)+'</label><input id="alSeg'+s.num+'" data-seg="'+esc(s.code)+'" list="alDl'+s.num+'" placeholder="any">'+dl(s)+'</div>';}).join('')+'</div></div>';
  h+='<div class="gs-fb">'+(others.length?'<button type="button" class="gf-open" id="alMoreBtn">More segments (branch, cost centre ...)</button>':'<span></span>')
    +'<div class="gp-act"><button type="button" class="gp-btn gp-btn--sm gp-btn--pri" id="alGo">'+IC.search+'<span>Search</span></button></div></div>';
  $('#alF').html(h);
  var m=/account=([^&]+)/.exec(location.hash||'');if(m){$('#alAcc').val(decodeURIComponent(m[1]));$('#alFrom').val((d.periods||[])[d.periods.length-1]||cur);}
  var t=/tran=([^&]+)/.exec(location.hash||'');if(t){$('#alQ').val(decodeURIComponent(t[1]));$('#alFrom').val((d.periods||[])[d.periods.length-1]||cur);}
  return !!(m||t);
}
function search(first){call('J52_SEARCH',{x01:JSON.stringify(filters()),x02:S.init?'':'init'},function(d){
  if(!S.init){S.d=d;S.init=true;if(renderFilters()){search();return;}if(first){$('#alRows').html('<tr><td colspan="9"><div class="gp-empty"><b>Choose an account</b>Type an account code (or the first digits of a group of accounts) and click Search.</div></td></tr>');return;}}
  render(d);});}
function render(d){var r=d.rows||[];
  $('#alSum').html('<div><small>Brought forward (before '+esc(d.from||'')+')</small><b>'+drcr(d.broughtForward)+'</b></div><div><small>Debits in the range</small><b>'+money(d.totalDr)+'</b></div>'
    +'<div><small>Credits in the range</small><b>'+money(d.totalCr)+'</b></div><div class="end"><small>Balance at the end of '+esc(d.to||'')+'</small><b>'+drcr(d.closing)+'</b></div>');
  $('#alCount').text(r.length?(r.length+(d.more?'+':'')+' movement'+(r.length===1?'':'s')+(d.more?' (first '+r.length+' shown)':'')):'Movements');
  if(!r.length){$('#alRows').html('<tr><td colspan="9"><div class="gp-empty"><b>No posted movement</b>Nothing was posted on this account in the chosen periods.</div></td></tr>');return;}
  $('#alRows').html('<tr class="bf"><td colspan="7">Brought forward</td><td></td><td class="gj-money">'+drcr(d.broughtForward)+'</td></tr>'
    +r.map(function(x){return '<tr'+(x.match?' class="hl"':'')+'><td style="white-space:nowrap">'+fd(x.date)+'</td>'
      +'<td><a class="jn" href="'+esc(x.url)+'" style="text-decoration:none">'+esc(x.no)+'</a><small>'+esc(x.name||'')+'</small></td>'
      +'<td>'+esc(x.tran||'')+'</td><td><code>'+esc(x.concat)+'</code></td><td>'+esc(x.descr||'')+'</td>'
      +'<td class="gj-money">'+(+x.dr?money(x.dr):'')+'</td><td class="gj-money">'+(+x.cr?money(x.cr):'')+'</td>'
      +'<td class="gj-money">'+drcr(x.before)+'</td><td class="gj-money bal">'+drcr(x.after)+'</td></tr>';}).join(''));
}
$(document).on('click','#alGo',function(){search();});
$(document).on('keydown','#alF input',function(e){if(e.key==='Enter'){e.preventDefault();search();}});
$(document).on('click','#alMoreBtn',function(){$('#alMore').toggleClass('show');});
$(function(){search(true);});
})(apex.jQuery);
});
