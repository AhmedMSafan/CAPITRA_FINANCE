apex.jQuery(function(){
var IC={edit:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 20h9"/><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z"/></svg>',plus:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 5v14M5 12h14"/></svg>'};
var TO='<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg>';
var S={rows:[],k:'ALL',q:'',ev:'',to:'',locs:{},sts:{}};
function esc(v){return apex.util.escapeHTML(v==null?'':String(v));}
function clean(m){return String(m||'').replace(/^ORA-\d+:\s*/,'').replace(/\nORA-[\s\S]*$/,'');}
function list(v){return String(v||'').split(/[\s,;:|]+/).filter(Boolean);}
function lbl(v){v=String(v||'').replace(/_/g,' ').toLowerCase();return v.replace(/\b\w/g,function(c){return c.toUpperCase();});}
var STC={DEPOSITED:'#3730A3',COLLECTED:'#15803D',REJECTED:'#B42318',RECEIVED:'#B45309'};function stc(s){return STC[s]||'#00636D';}
function locB(c){return '<span class="cx-loc '+esc(c)+'">'+esc(S.locs[c]||lbl(c))+'</span>';}
var KP=[['ALL','All actions','#5F7B8A',function(){return true;}],['ON','Active','#14734F',function(r){return r.active==='Y';}],
  ['OFF','Inactive','#6B7280',function(r){return r.active!=='Y';}],['REQ','Need extra input','#E87722',function(r){return r.reqDep==='Y'||r.reqBank==='Y'||r.reqReason==='Y'||r.reqNotes==='Y';}]];
function kf(k){for(var i=0;i<KP.length;i++)if(KP[i][0]===k)return KP[i][3];return KP[0][3];}
function shown(){var q=S.q.toLowerCase(),f=kf(S.k);return S.rows.filter(function(r){if(!f(r))return false;if(S.ev&& r.event!==S.ev)return false;if(S.to&& r.to!==S.to)return false;
  return !q||[r.code,r.name,r.nameAr,r.event,r.eventName,r.to,S.locs[r.to],r.fromStatus,r.fromLoc,r.proc].join(' ').toLowerCase().indexOf(q)>=0;});}
function tiles(){$('#ctKpis').html(KP.map(function(x){return '<button type="button" class="cb-k'+(S.k===x[0]?' on':'')+'" data-k="'+x[0]+'" style="--c:'+x[2]+'"><span class="h"><i></i>'+esc(x[1])+'</span><b>'+S.rows.filter(x[3]).length+'</b></button>';}).join(''));}
function card(r){var from=list(r.fromLoc),st=list(r.fromStatus),req=[];
  if(r.reqDep==='Y')req.push('Deposit batch');if(r.reqBank==='Y')req.push('Bank reference');if(r.reqReason==='Y')req.push('Rejection reason');if(r.reqNotes==='Y')req.push('Notes');
  return '<article class="ct-card'+(r.active==='Y'?'':' off')+'"><div class="ct-top"><span class="ct-seq" title="Display order">'+esc(r.seq==null?'-':r.seq)+'</span>'
    +'<div class="ct-name"><b>'+esc(r.name||r.code)+'</b>'+(r.nameAr?'<small class="ar">'+esc(r.nameAr)+'</small>':'')+'<small class="ct-code">'+esc(r.code)+'</small></div>'
    +'<span class="ct-on'+(r.active==='Y'?'':' no')+'">'+(r.active==='Y'?'Active':'Inactive')+'</span></div>'
    +'<div class="ct-ev"><span>Event</span><b>'+esc(r.eventName||lbl(r.event)||'-')+'</b><small>'+esc(r.event||'')+'</small></div>'
    +'<div class="ct-blk"><div class="ct-row"><span>New cheque status</span>'+(S.sts[r.event]?'<span class="cw-st" style="--c:'+stc(r.event)+'">'+esc(S.sts[r.event])+'</span>':'<span class="ct-chip none">No change</span>')+'</div>'
    +'<div class="ct-row"><span>Allowed from status</span><span class="ct-chips">'+(st.length?st.map(function(s){return '<span class="ct-chip">'+esc(S.sts[s]||lbl(s))+'</span>';}).join(''):'<span class="ct-chip none">Any status</span>')+'</span></div></div>'
    +'<div class="ct-blk"><div class="ct-row"><span>New location</span>'+(r.to?locB(r.to):'<span class="ct-chip none">No change</span>')+'</div>'
    +'<div class="ct-row"><span>Allowed from location</span><span class="ct-chips">'+(from.length?from.map(locB).join(''):'<span class="ct-chip none">Any location</span>')+'</span></div></div>'
    +'<div class="ct-blk ct-sm"><div class="ct-row"><span>Requires</span><span class="ct-chips">'+(req.length?req.map(function(s){return '<span class="ct-chip req">'+s+'</span>';}).join(''):'<span class="ct-chip none">Nothing extra</span>')+'</span></div>'
    +'<div class="ct-row"><span>Procedure</span><span class="ct-proc">'+esc(r.proc||'-')+'</span></div></div>'
    +'<div class="ct-foot"><span>'+(r.updated?'Updated '+esc(r.updated)+(r.updatedBy?' by '+esc(r.updatedBy):''):(r.created?'Created '+esc(r.created)+(r.createdBy?' by '+esc(r.createdBy):''):''))+'</span>'
    +'<a class="gp-btn" href="'+esc(r.url)+'">'+IC.edit+'<span>Edit</span></a></div></article>';}
function render(){var rows=shown();$('#ctCount').text(rows.length+' of '+S.rows.length+' actions');$('#ctClear').toggleClass('show',!!(S.q||S.ev||S.to||S.k!=='ALL'));
  if(!rows.length){$('#ctList').html('<div class="cb-list"><div class="cb-empty"><b>'+(S.rows.length?'No action matches':'No action yet')+'</b>'+(S.rows.length?'Change the search, the filters or the tile.':'Use New action to add the first one.')+'</div></div>');return;}
  $('#ctList').html('<div class="ct-grid">'+rows.map(card).join('')+'</div>');}
$('#ctKpis').on('click','.cb-k',function(){S.k=String($(this).data('k'));tiles();render();});
$('#ctQ').on('input',function(){S.q=$.trim(this.value);render();});
$('#ctEv').on('change',function(){S.ev=this.value;render();});
$('#ctTo').on('change',function(){S.to=this.value;render();});
$('#ctClear').on('click',function(){S.q=S.ev=S.to='';S.k='ALL';$('#ctQ').val('');$('#ctEv').val('');$('#ctTo').val('');tiles();render();});
$('#ctCsv').on('click',function(){var q=function(v){v=v==null?'':String(v);return /[",\n;]/.test(v)?'"'+v.replace(/"/g,'""')+'"':v;};
  var csv=[['Order','Action code','Name','Arabic name','Event type','Target location','Allowed from status','Allowed from location','Requires deposit batch','Requires bank ref','Requires rejection reason','Requires notes','Procedure','Active'].join(',')]
    .concat(shown().map(function(r){return [r.seq,r.code,r.name,r.nameAr,r.event,r.to,r.fromStatus,r.fromLoc,r.reqDep,r.reqBank,r.reqReason,r.reqNotes,r.proc,r.active].map(q).join(',');})).join('\r\n');
  var b=new Blob(['﻿'+csv],{type:'text/csv;charset=utf-8'}),a=document.createElement('a');a.href=URL.createObjectURL(b);a.download='cheque-action-catalog.csv';document.body.appendChild(a);a.click();
  setTimeout(function(){URL.revokeObjectURL(a.href);a.remove();},500);});
function load(){apex.server.process('CHQ_ACTIONS',{},{dataType:'json'}).then(function(d){if(!d||d.ok===false){$('#ctList').html('<div class="cb-list"><div class="cb-empty"><b>The actions could not be loaded</b>'+esc(clean(d&& d.message))+'</div></div>');return;}
    S.rows=d.rows||[];S.locs={};S.sts={};(d.locations||[]).forEach(function(x){S.locs[x[0]]=x[1];});(d.statuses||[]).forEach(function(x){S.sts[x[0]]=x[1];});$('#ctNew').attr('href',d.newUrl||'#');
    var e=$('#ctEv').val(),t=$('#ctTo').val(),E={},T={};S.rows.forEach(function(r){if(r.event)E[r.event]=r.eventName||lbl(r.event);if(r.to)T[r.to]=S.locs[r.to]||r.to;});
    $('#ctEv').html('<option value="">All event types</option>'+Object.keys(E).map(function(k){return '<option value="'+esc(k)+'">'+esc(E[k])+'</option>';}).join('')).val(e);
    $('#ctTo').html('<option value="">All target locations</option>'+Object.keys(T).map(function(k){return '<option value="'+esc(k)+'">'+esc(T[k])+'</option>';}).join('')).val(t);
    tiles();render();},
  function(x){$('#ctList').html('<div class="cb-list"><div class="cb-empty"><b>The actions could not be loaded</b>'+esc(clean(x&& x.responseText).substring(0,300))+'</div></div>');});}
// page 5 is a dialog: reload the cards when it closes (same as the former 'Dialog Closed' refresh)
$('#ctApp').on('apexafterclosedialog',load);$(window).on('apexafterclosedialog',load);
load();
});
