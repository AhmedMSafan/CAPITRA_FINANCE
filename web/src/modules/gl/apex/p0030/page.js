(function($){
'use strict';
var S={calId:null,cals:[],ledgers:[],years:[],periods:[],st:[],yst:[],types:[],freqs:[],year:null,onOk:null};
var ICO_EDIT='<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 20h9"/><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z"/></svg>',ICO_PLUS='<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 5v14M5 12h14"/></svg>',ICO_SYNC='<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M21 12a9 9 0 0 1-15.5 6.2L3 16"/><path d="M3 12a9 9 0 0 1 15.5-6.2L21 8"/><path d="M21 3v5h-5M3 21v-5h5"/></svg>';
function esc(v){return apex.util.escapeHTML(v==null?'':String(v));}
function lbl(v){v=String(v||'').replace(/_/g,' ').toLowerCase();return v.replace(/\b\w/g,function(c){return c.toUpperCase();});}
function cls(s){s=String(s||'');return s==='OPEN'?'ok':(s.indexOf('CLOSE')>=0||!s?'off':'');}
function pill(s){return '<span class="gp-pill '+cls(s)+'">'+esc(s?lbl(s):'-')+'</span>';}
function fd(d){if(!d)return '-';var p=d.split('-');var m=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];return p[2]+' '+m[+p[1]-1]+' '+p[0];}
function today(){if(S.today)return S.today;var t=new Date();function z(n){return (n<10?'0':'')+n;}return t.getFullYear()+'-'+z(t.getMonth()+1)+'-'+z(t.getDate());}
function cal(){return S.cals.find(function(c){return String(c.id)===String(S.calId);});}
function call(name,data,ok){
  apex.server.process(name,data,{dataType:'json'}).then(function(d){
    if(d&& d.ok===false){showErr(d.message||'The request failed.');return;}ok(d||{});
  },function(x){showErr((x&& x.responseText)||'The request failed.');});
}
function showErr(m){m=String(m||'').replace(/^ORA-\d+:\s*/,'').replace(/\nORA-[\s\S]*$/,'');
  if($('#gpMb').hasClass('open')){$('#gpErr').text(m).addClass('show');$('#gpMok').prop('disabled',false);}else{apex.message.clearErrors();apex.message.showErrors([{type:'error',location:'page',message:m,unsafe:false}]);}}
function load(calId,keepYear){
  call('PER_LOAD',{x01:calId||''},function(d){
    S.today=d.today||null;S.calId=d.calId;S.cals=d.calendars||[];S.ledgers=d.ledgers||[];S.years=d.years||[];S.periods=d.periods||[];
    S.st=d.statuses||[];S.yst=d.yearStatuses||[];S.types=d.types||[];S.freqs=d.freqs||[];
    if(!keepYear||!S.years.some(function(y){return String(y.id)===String(S.year);})){
      var t=today(),cur=S.years.find(function(y){return y.start<=t&& y.end>=t;});S.year=cur?cur.id:(S.years[0]?S.years[0].id:null);}
    render();
  });
}
function render(){
  $('#gpCal').html(S.cals.length?S.cals.map(function(c){return '<option value="'+c.id+'"'+(String(c.id)===String(S.calId)?' selected':'')+'>'+esc(c.code+' - '+c.nameEn)+'</option>';}).join(''):'<option value="">No calendar yet</option>');
  $('#gpCalEdit,#gpGen').prop('disabled',!S.calId);
  var c=cal(),h='';
  if(c){h='<div class="gp-cal"><div><span class="k">Calendar</span><span class="v">'+esc(c.nameEn)+(c.nameAr?' <span style="font-weight:500;color:#5F7B8A">&middot; '+esc(c.nameAr)+'</span>':'')+'</span></div>'
    +'<div><span class="k">Type</span><span class="v">'+esc(lbl(c.type))+'</span></div><div><span class="k">Frequency</span><span class="v">'+esc(lbl(c.freq))+'</span></div>'
    +'<div><span class="k">Adjustment period</span><span class="v">'+(c.adj==='Y'?'Yes':'No')+'</span></div><div><span class="k">Status</span><span class="v">'+(c.active==='Y'?pill('ACTIVE').replace('gp-pill ','gp-pill ok '):'<span class="gp-pill off">Inactive</span>')+'</span></div>'
    +'<div class="led"><span class="k" style="margin-right:4px">Ledgers on this calendar</span>'+(S.ledgers.length?S.ledgers.map(function(l){return '<span class="gp-tag">'+esc(l.code)+'</span>';}).join(''):'<span class="gp-tag">None</span>')+'</div></div>';}
  var tp=S.periods.find(function(p){return p.adj!=='Y'&& p.start<=today()&& p.end>=today();});
  if(tp){var notOpen=(tp.ledgers||[]).filter(function(l){return l.status!=='OPEN';});
    if(notOpen.length)h+='<div class="gp-alert"><b>Today\u2019s period '+esc(tp.code)+' is not open</b><span>'+notOpen.map(function(l){return esc(l.code)+': '+esc(lbl(l.status));}).join(' \u00b7 ')+'</span>'
      +notOpen.map(function(l){return '<button type="button" class="gp-btn gp-btn--sm gp-btn--pri" data-lp="'+l.lp+'">Open '+esc(l.code)+'</button>';}).join('')+'</div>';}
  else if(S.calId&& S.periods.length)h+='<div class="gp-alert"><b>No period covers today ('+fd(today())+')</b><span>Generate the fiscal year or add a period.</span></div>';
  $('#gpCalInfo').html(h);
  $('#gpYears').html(S.years.length?S.years.map(function(y){return '<button type="button" class="gp-yr'+(String(y.id)===String(S.year)?' on':'')+'" data-y="'+y.id+'"><span class="r1"><b>'+esc(y.name||y.year)+'</b>'+pill(y.status)+'</span>'
    +'<small>'+fd(y.start)+' &ndash; '+fd(y.end)+'</small><small>'+y.periods+' periods'+(function(){var n=Object.keys(issues(y)).length;return n?' \u00b7 <span style="color:#B42318;font-weight:700">'+n+' to check</span>':'';})()+'</small></button>';}).join(''):'<div class="gp-empty" style="background:#fff;border:1px dashed #DFE8EC;border-radius:14px">No fiscal years yet.</div>');
  renderPeriods();
}
function addDay(d,n){var p=d.split('-'),x=new Date(Date.UTC(+p[0],+p[1]-1,+p[2]+n));return x.toISOString().slice(0,10);}
function issues(y){ // data checks for the periods of fiscal year y: dates outside the year, overlaps, gaps, code/name year vs dates
  var out={},rows=S.periods.filter(function(p){return String(p.yearId)===String(y.id);});
  function add(id,m){(out[id]=out[id]||[]).push(m);}
  rows.forEach(function(p){
    if(p.start<y.start||p.end>y.end)add(p.id,'Dates are outside fiscal year '+(y.name||y.year)+' ('+fd(y.start)+' \u2013 '+fd(y.end)+').');
    if(p.end<p.start)add(p.id,'End date is before start date.');
    if(p.adj!=='Y'){S.periods.forEach(function(o){if(o.id!==p.id&& o.adj!=='Y'&& o.start<=p.end&& o.end>=p.start)add(p.id,'Overlaps period '+o.code+' ('+fd(o.start)+' \u2013 '+fd(o.end)+').');});
      var yr=p.start.slice(0,4),seen={};[p.code,p.nameEn,p.nameAr].forEach(function(t){(String(t||'').match(/(19|20)\d\d/g)||[]).forEach(function(x){if(x!==yr&&!seen[x]){seen[x]=1;add(p.id,(t===p.code?'Code':'Name')+' says '+x+' but the dates are in '+yr+'.');}});});}
  });
  var reg=rows.filter(function(p){return p.adj!=='Y'&& p.start>=y.start&& p.end<=y.end;}).sort(function(a,b){return a.start<b.start?-1:1;});
  for(var i=1;i<reg.length;i++){if(reg[i].start>addDay(reg[i-1].end,1))add(reg[i].id,'Gap: no period covers '+fd(addDay(reg[i-1].end,1))+' \u2013 '+fd(addDay(reg[i].start,-1))+'.');}
  if(reg.length&&reg[0].start>y.start)add(reg[0].id,'Gap: the year starts on '+fd(y.start)+' but the first period starts on '+fd(reg[0].start)+'.');
  if(reg.length&&reg[reg.length-1].end<y.end)add(reg[reg.length-1].id,'Gap: the year ends on '+fd(y.end)+' but the last period ends on '+fd(reg[reg.length-1].end)+'.');
  return out;}
function ledCell(p){ // each ledger: status + a direct Open / Close button
  if(!(p.ledgers||[]).length)return '<div class="gp-led"><span class="off"><i></i>Not created</span></div>';
  return '<div class="gp-lrows">'+p.ledgers.map(function(l){var open=l.status==='OPEN',perm=String(l.status).indexOf('PERMANENT')>=0;
    return '<div class="gp-lrow"><span class="gp-led"><button type="button" class="'+cls(l.status)+'" data-lp="'+l.lp+'" title="Details and all actions"><i></i>'+esc(l.code)+' \u00b7 '+esc(lbl(l.status))+'</button></span>'
      +(perm?'':open?'<button type="button" class="gp-btn gp-btn--sm" data-close="'+l.lp+'">Close</button>'
        :'<button type="button" class="gp-btn gp-btn--sm gp-btn--pri" data-open="'+l.lp+'" data-code="'+esc(p.code)+'" data-led="'+esc(l.code)+'">Open</button>')+'</div>';}).join('')+'</div>';}
function quickOpen(lp,code,led){apex.message.confirm('Open period '+code+' for '+led+'?',function(yes){if(!yes)return;
  call('PER_LP_ACTION',{x01:lp,x02:'OPEN',x03:'Opened from the periods page'},function(x){close();apex.message.showPageSuccess(led+' '+code+' is now '+lbl(x.status)+'.');load(S.calId,true);});});}
function renderPeriods(){
  var y=S.years.find(function(x){return String(x.id)===String(S.year);});
  if(!S.calId){$('#gpPanel').html('<div class="gp-empty"><b>No calendar defined</b>Create a calendar first with <b style="display:inline">New calendar</b>.</div>');return;}
  if(!y){$('#gpPanel').html('<div class="gp-empty"><b>No fiscal year selected</b>Use <b style="display:inline">Generate fiscal year</b> to create 12 monthly periods (and an adjustment period) in one step.</div>');return;}
  var rows=S.periods.filter(function(p){return String(p.yearId)===String(y.id);}),t=today(),iss=issues(y),ni=Object.keys(iss).length;
  var h='<div class="gp-ph"><h3>Fiscal year '+esc(y.name||y.year)+'<small>'+rows.length+' periods &middot; '+fd(y.start)+' &ndash; '+fd(y.end)+'</small></h3><div class="gp-act">'
    +'<button type="button" class="gp-btn gp-btn--sm" id="gpSync"'+(S.ledgers.length?'':' disabled title="No ledger uses this calendar"')+'>'+ICO_SYNC+'<span>Create ledger periods</span></button>'
    +'<button type="button" class="gp-btn gp-btn--sm" id="gpYearEdit">'+ICO_EDIT+'<span>Edit year</span></button>'
    +'<button type="button" class="gp-btn gp-btn--sm" id="gpAdd">'+ICO_PLUS+'<span>Add period</span></button></div></div>';
  if(ni)h+='<div class="gp-warn"><b>'+ni+' period'+(ni>1?'s need':' needs')+' attention.</b> The data below does not match the fiscal year; rows are marked. Fix them with the edit button (dates and names) or ask for a data fix for codes.</div>';
  if(!rows.length){h+='<div class="gp-empty">This fiscal year has no periods.</div>';}
  else{h+='<div class="gp-tw"><table class="gp-t"><thead><tr><th>#</th><th>Code</th><th>Name</th><th>Start</th><th>End</th><th>Qtr</th><th>Default status</th><th>Ledger periods</th><th></th></tr></thead><tbody>';
    rows.forEach(function(p){var now=p.adj!=='Y'&& p.start<=t&& p.end>=t;
      var pi=iss[p.id]||[];
      h+='<tr class="'+(p.adj==='Y'?'adj':'')+(now?' now':'')+(pi.length?' bad':'')+'"><td class="num">'+esc(p.no)+'</td><td><code>'+esc(p.code)+'</code>'+(now?'<span class="gp-today">Today</span>':'')+'</td><td>'+esc(p.nameEn)+(p.adj==='Y'?' <span class="gp-tag">Adjustment</span>':'')+(p.nameAr?'<span class="ar">'+esc(p.nameAr)+'</span>':'')
        +(pi.length?'<ul class="gp-iss">'+pi.map(function(m){return '<li>'+esc(m)+'</li>';}).join('')+'</ul>':'')+'</td>'
        +'<td class="num">'+fd(p.start)+'</td><td class="num">'+fd(p.end)+'</td><td class="num">'+(p.qtr?'Q'+esc(p.qtr):'-')+'</td><td>'+pill(p.status)+'</td>'
        +'<td>'+ledCell(p)+'</td>'
        +'<td><button type="button" class="gp-btn gp-btn--sm" data-edit="'+p.id+'" aria-label="Edit '+esc(p.code)+'">'+ICO_EDIT+'</button></td></tr>';});
    h+='</tbody></table></div>';}
  h+='<div class="gp-note" style="padding:0 16px 14px">"Today" is the database date ('+fd(t)+'). The status is not set by the date: it is stored per period, and the default status is copied to a ledger period when it is first created. After that each ledger keeps its own status: click a ledger to open, close or reopen its period.</div>';
  $('#gpPanel').html(h);
}
/* ---------- modal */
function opts(list,sel,extra){var a=(list||[]).slice();(extra||[]).forEach(function(x){if(x&& a.indexOf(x)<0)a.push(x);});if(sel&& a.indexOf(sel)<0)a.push(sel);
  return a.map(function(v){return '<option value="'+esc(v)+'"'+(v===sel?' selected':'')+'>'+esc(lbl(v))+'</option>';}).join('');}
function modal(title,body,okLabel,onOk){$('#gpMt').text(title);$('#gpMc').html(body);$('#gpErr').removeClass('show').text('');$('#gpMok').prop('disabled',false).text(okLabel||'Save').toggle(!!onOk);S.onOk=onOk;
  $('#gpMb').addClass('open');setTimeout(function(){$('#gpMc').find('input:not([readonly]),select').first().trigger('focus');},40);}
function close(){$('#gpMb').removeClass('open');S.onOk=null;}
function v(id){return $.trim($('#'+id).val()||'');}
function yn(id){return $('#'+id).is(':checked')?'Y':'N';}
function calForm(c){
  var e=!!c;c=c||{type:'MONTHLY',freq:'MONTHLY',adj:'Y',active:'Y'};
  modal(e?'Edit calendar':'New calendar',
    '<div class="gp-g"><div class="gp-f"><label for="fCode">Calendar code *</label><input type="text" id="fCode" maxlength="30" value="'+esc(c.code||'')+'"'+(e?' readonly':'')+'>'+(e?'<div class="gp-hint">The code identifies the calendar and cannot be changed.</div>':'')+'</div>'
    +'<div class="gp-f"><label for="fNameEn">Name (English) *</label><input type="text" id="fNameEn" maxlength="200" value="'+esc(c.nameEn||'')+'"></div>'
    +'<div class="gp-f full"><label for="fNameAr">Name (Arabic)</label><input type="text" id="fNameAr" class="rtl" maxlength="200" value="'+esc(c.nameAr||'')+'"></div>'
    +'<div class="gp-f"><label for="fType">Calendar type</label><select id="fType">'+opts(S.types,c.type,['MONTHLY'])+'</select></div>'
    +'<div class="gp-f"><label for="fFreq">Period frequency</label><select id="fFreq">'+opts(S.freqs,c.freq,['MONTHLY'])+'</select></div>'
    +'<label class="gp-chk"><input type="checkbox" id="fAdj"'+(c.adj==='Y'?' checked':'')+'> Uses an adjustment period</label>'
    +'<label class="gp-chk"><input type="checkbox" id="fAct"'+(c.active==='Y'?' checked':'')+'> Active</label></div>',
    'Save calendar',function(){
      if(!v('fCode')||!v('fNameEn')){showErr('Calendar code and English name are required.');return;}
      $('#gpMok').prop('disabled',true);
      call('PER_SAVE_CAL',{x01:v('fCode'),x02:v('fNameEn'),x03:v('fNameAr'),x04:v('fType'),x05:v('fFreq'),x06:yn('fAdj'),x07:yn('fAct')},function(d){close();apex.message.showPageSuccess('Calendar saved.');load(d.id,true);});
    });
}
function genPreview(){var y=+v('gY'),s=v('gS');if(!s){$('#gPrev').html('');return;}var p=s.split('-'),d=new Date(+p[0],+p[1]-1,+p[2]),m=['JAN','FEB','MAR','APR','MAY','JUN','JUL','AUG','SEP','OCT','NOV','DEC'],h='';
  for(var i=0;i<12;i++){var a=new Date(d.getFullYear(),d.getMonth()+i,d.getDate());var b=new Date(a.getFullYear(),a.getMonth()+1,0);h+='<span><b>'+m[a.getMonth()]+'-'+(y||'????')+'</b>'+a.getDate()+'/'+(a.getMonth()+1)+' &ndash; '+b.getDate()+'/'+(b.getMonth()+1)+'</span>';}
  if($('#gAdj').is(':checked'))h+='<span><b>ADJ-'+(y||'????')+'</b>Adjustment</span>';$('#gPrev').html(h);}
function genForm(){
  var c=cal();if(!c)return;var last=S.years.reduce(function(a,y){return (!a||+y.year>+a.year)?y:a;},null),ny,ns;
  if(last){ny=+last.year+1;var e=last.end.split('-'),nd=new Date(+e[0],+e[1]-1,+e[2]+1);ns=nd.getFullYear()+'-'+('0'+(nd.getMonth()+1)).slice(-2)+'-'+('0'+nd.getDate()).slice(-2);}
  else{ny=new Date().getFullYear();ns=ny+'-01-01';}
  modal('Generate fiscal year \u00b7 '+c.code,
    '<div class="gp-g"><div class="gp-f"><label for="gY">Fiscal year *</label><input type="number" id="gY" min="1990" max="2200" value="'+ny+'"></div>'
    +'<div class="gp-f"><label for="gS">First day of the year *</label><input type="date" id="gS" value="'+ns+'"><div class="gp-hint">Twelve monthly periods are created from this date; each ends on the last day of its month.</div></div>'
    +'<div class="gp-f"><label for="gYs">Fiscal year status</label><select id="gYs">'+opts(YEAR_STATUSES,'OPEN',[])+'</select></div><div></div>'
    +'<label class="gp-chk"><input type="checkbox" id="gAdj"'+(c.adj==='Y'?' checked':'')+'> Add an adjustment period (13)</label>'
    +'<label class="gp-chk"><input type="checkbox" id="gSync"'+(S.ledgers.length?' checked':' disabled')+'> Create ledger periods for '+(S.ledgers.length?S.ledgers.length+' ledger(s)':'ledgers (none on this calendar)')+'</label>'
    +'<div class="gp-f full"><div class="gp-hint">New periods start as <b>Never Opened</b>. Ledger periods also get their source status and close checklist rows.</div><div class="gp-prev" id="gPrev"></div></div></div>',
    'Generate',function(){
      if(!v('gY')||!v('gS')){showErr('Fiscal year and first day are required.');return;}
      $('#gpMok').prop('disabled',true);
      call('PER_GENERATE',{x01:c.code,x02:v('gY'),x03:v('gS'),x04:yn('gAdj'),x05:v('gYs'),x06:yn('gSync')},function(d){close();apex.message.showPageSuccess(d.message||'Fiscal year generated.');S.year=d.yearId;load(S.calId,true);});
    });
  $('#gY,#gS,#gAdj').on('input change',genPreview);genPreview();
}
function periodForm(p){
  var c=cal(),y=S.years.find(function(x){return String(x.id)===String(S.year);});if(!c||!y)return;var e=!!p;
  if(!e){var rows=S.periods.filter(function(r){return String(r.yearId)===String(y.id);});p={no:rows.reduce(function(a,r){return Math.max(a,+r.no||0);},0)+1,status:'NEVER_OPENED',adj:'N',start:y.start,end:y.end};}
  modal(e?'Edit period '+p.code:'Add period to '+(y.name||y.year),
    '<div class="gp-g"><div class="gp-f"><label for="pCode">Period code *</label><input type="text" id="pCode" maxlength="30" value="'+esc(p.code||'')+'"'+(e?' readonly':'')+'>'+(e?'<div class="gp-hint">The code identifies the period and cannot be changed.</div>':'')+'</div>'
    +'<div class="gp-f"><label for="pNo">Period number *</label><input type="number" id="pNo" min="1" max="99" value="'+esc(p.no)+'"></div>'
    +'<div class="gp-f"><label for="pNameEn">Name (English) *</label><input type="text" id="pNameEn" maxlength="80" value="'+esc(p.nameEn||'')+'"></div>'
    +'<div class="gp-f"><label for="pNameAr">Name (Arabic)</label><input type="text" id="pNameAr" class="rtl" maxlength="80" value="'+esc(p.nameAr||'')+'"></div>'
    +'<div class="gp-f"><label for="pStart">Start date *</label><input type="date" id="pStart" value="'+esc(p.start||'')+'"></div>'
    +'<div class="gp-f"><label for="pEnd">End date *</label><input type="date" id="pEnd" value="'+esc(p.end||'')+'"></div>'
    +'<div class="gp-f"><label for="pQ">Quarter</label><select id="pQ"><option value="">-</option>'+[1,2,3,4].map(function(q){return '<option value="'+q+'"'+(String(p.qtr)===String(q)?' selected':'')+'>Q'+q+'</option>';}).join('')+'</select></div>'
    +'<div class="gp-f"><label for="pSt">Default status for new ledger periods</label><select id="pSt">'+opts(S.st,p.status,['NEVER_OPENED','FUTURE_ENTRY','OPEN','CLOSED'])+'</select>'
    +'<div class="gp-hint">Used only when a ledger period is first created. To open or close the period now, use the ledger buttons below.</div></div>'
    +'<label class="gp-chk"><input type="checkbox" id="pAdj"'+(p.adj==='Y'?' checked':'')+'> Adjustment period</label><div></div>'
    +(e&&(p.ledgers||[]).length?'<div class="gp-f full"><label>Ledger periods (open / close now)</label>'+ledCell(p)+'</div>':'')
    +'<div class="gp-f full"><div class="gp-hint">Dates must fall inside fiscal year '+esc(y.name||y.year)+' ('+fd(y.start)+' &ndash; '+fd(y.end)+') and must not overlap another regular period of this calendar.</div></div></div>',
    'Save period',function(){
      if(!v('pCode')||!v('pNo')||!v('pNameEn')||!v('pStart')||!v('pEnd')){showErr('Code, number, English name, start and end dates are required.');return;}
      $('#gpMok').prop('disabled',true);
      call('PER_SAVE_PERIOD',{x01:e?p.id:'',x02:c.code,x03:y.year,x04:v('pNo'),x05:v('pCode'),x06:v('pNameEn'),x07:v('pNameAr'),x08:v('pStart'),x09:v('pEnd'),x10:v('pQ'),x11:yn('pAdj'),x12:v('pSt')},
        function(){close();apex.message.showPageSuccess('Period saved.');load(S.calId,true);});
    });
}
function sync(){
  var c=cal(),y=S.years.find(function(x){return String(x.id)===String(S.year);});if(!c||!y)return;
  apex.message.confirm('Create the missing ledger periods, source status and close checklist rows of fiscal year '+(y.name||y.year)+' for '+S.ledgers.length+' ledger(s)? Existing rows are not changed.',function(ok){
    if(!ok)return;call('PER_SYNC',{x01:c.id,x02:y.year},function(d){apex.message.showPageSuccess(d.message||'Ledger periods synchronised.');load(S.calId,true);});});
}
var YEAR_STATUSES=['DRAFT','OPEN','CLOSED','PERMANENTLY_CLOSED'];
var ACTS=[['OPEN','Open period','gp-btn--pri'],['SET_FUTURE','Allow future entry',''],['CLOSE','Close period',''],['REOPEN','Reopen',''],['PERMANENT','Close permanently','gp-btn--danger']];
function yearForm(){
  var c=cal(),y=S.years.find(function(x){return String(x.id)===String(S.year);});if(!c||!y)return;
  modal('Edit fiscal year '+(y.name||y.year),
    '<div class="gp-g"><div class="gp-f"><label for="yNo">Fiscal year</label><input type="number" id="yNo" value="'+esc(y.year)+'" readonly></div>'
    +'<div class="gp-f"><label for="yName">Name</label><input type="text" id="yName" maxlength="50" value="'+esc(y.name||'')+'"></div>'
    +'<div class="gp-f"><label for="yStart">Start date *</label><input type="date" id="yStart" value="'+esc(y.start)+'"></div>'
    +'<div class="gp-f"><label for="yEnd">End date *</label><input type="date" id="yEnd" value="'+esc(y.end)+'"></div>'
    +'<div class="gp-f"><label for="ySt">Status *</label><select id="ySt">'+opts(YEAR_STATUSES,y.status,[])+'</select></div><div></div>'
    +'<div class="gp-f full"><div class="gp-hint">The fiscal-year status is a label for the year. Posting is controlled by each ledger period (the ledger tags in the table). '
    +'The dates must still contain all '+esc(y.periods)+' periods of the year.</div></div></div>',
    'Save year',function(){
      if(!v('yStart')||!v('yEnd')){showErr('Start and end dates are required.');return;}
      if(v('yEnd')<v('yStart')){showErr('The end date cannot be before the start date.');return;}
      $('#gpMok').prop('disabled',true);
      call('PER_SAVE_YEAR',{x01:c.code,x02:y.year,x03:v('yName'),x04:v('yStart'),x05:v('yEnd'),x06:v('ySt')},function(){close();apex.message.showPageSuccess('Fiscal year saved.');load(S.calId,true);});
    });
}
function lpForm(lp){
  call('PER_LP_INFO',{x01:lp},function(d){
    var st=d.status||'',h='<div class="gp-kv"><div><small>Status</small><b>'+pill(st)+'</b></div><div><small>Opened</small><b>'+esc(d.openedBy||'-')+'</b><small>'+esc(d.openedDt||'')+'</small></div>'
      +'<div><small>Closed</small><b>'+esc(d.closedBy||d.permBy||'-')+'</b><small>'+esc(d.closedDt||d.permDt||'')+'</small></div></div>';
    if(st!=='OPEN'&&st!=='FUTURE_ENTRY'&&st!=='NEVER_OPENED'&& d.next&& d.next.status!=='OPEN')
      h+='<div class="gp-ready ok"><b>Next period:</b> '+esc(d.next.code)+' is '+esc(lbl(d.next.status))+'. <button type="button" class="gp-btn gp-btn--sm gp-btn--pri" id="lpNext" style="margin-left:6px">Open '+esc(d.next.code)+'</button></div>';
    if(st==='OPEN'){var ok=String(d.canClose||'').toUpperCase().charAt(0)==='Y';
      h+='<div class="gp-ready '+(ok?'ok':'no')+'"><b>'+(ok?'Ready to close.':'Not ready to close.')+'</b> '+esc(d.canCloseMsg||'')+(d.blockers!=null?' · '+esc(d.blockers)+' blocker(s)':'')+'</div>';}
    if(d.checklist&& d.checklist.length)h+='<div class="gp-led" style="margin-bottom:12px">'+d.checklist.map(function(c){return '<span class="off"><i></i>Checklist: '+esc(lbl(c.status))+' · '+esc(c.n)+'</span>';}).join('')+'</div>';
    h+='<div class="gp-f"><label for="lpReason">Reason / notes</label><textarea id="lpReason" maxlength="4000" placeholder="Required to reopen or permanently close"></textarea></div>';
    h+='<div class="gp-acts">'+ACTS.map(function(a){var same=(a[0]==='OPEN'&&st==='OPEN')||(a[0]==='SET_FUTURE'&&st==='FUTURE_ENTRY')||(a[0]==='PERMANENT'&& st.indexOf('PERMANENT')>=0);
      return '<button type="button" class="gp-btn '+a[2]+'" data-act="'+a[0]+'"'+(same?' disabled':'')+'>'+a[1]+'</button>';}).join('')+'</div>';
    if(d.history&& d.history.length)h+='<div class="gp-hist"><h4>History</h4>'+d.history.map(function(x){return '<div><span><b>'+esc(lbl(x.action))+'</b> '+esc(lbl(x.from||'-'))+' &rarr; '+esc(lbl(x.to))+(x.reason?' · '+esc(x.reason):'')+'</span><small>'+esc(x.by)+' · '+esc(x.at)+'</small></div>';}).join('')+'</div>';
    modal(d.ledger+' · '+d.period,h,null,null);
    $('#gpMc').off('click.lpn').on('click.lpn','#lpNext',function(){var n=d.next;apex.message.confirm('Open period '+n.code+' for '+d.ledger+'?',function(yes){if(!yes)return;
      call('PER_LP_ACTION',{x01:n.lp,x02:'OPEN',x03:'Opened after closing '+d.period},function(x){close();apex.message.showPageSuccess(d.ledger+' '+n.code+' is now '+lbl(x.status)+'.');load(S.calId,true);});});});
    $('#gpMc').off('click.lp').on('click.lp','[data-act]',function(){
      var act=$(this).data('act'),r=$.trim($('#lpReason').val()),lb=$(this).text();
      if((act==='REOPEN'||act==='PERMANENT')&&!r){showErr('Please enter a reason.');return;}
      apex.message.confirm(lb+': '+d.ledger+' '+d.period+'?'+(act==='PERMANENT'?' This cannot be undone.':''),function(yes){
        if(!yes)return;call('PER_LP_ACTION',{x01:lp,x02:act,x03:r},function(x){close();apex.message.showPageSuccess(d.ledger+' '+d.period+' is now '+lbl(x.status)+'.');load(S.calId,true);
          if(act==='CLOSE'&& x.next&& x.next.status!=='OPEN')setTimeout(function(){apex.message.confirm(d.period+' is closed. Open the next period '+x.next.code+' ('+lbl(x.next.status)+') for '+d.ledger+' now?',function(y2){if(!y2)return;
            call('PER_LP_ACTION',{x01:x.next.lp,x02:'OPEN',x03:'Opened after closing '+d.period},function(z){apex.message.showPageSuccess(d.ledger+' '+x.next.code+' is now '+lbl(z.status)+'.');load(S.calId,true);});});},400);});});
    });
  });
}
$(function(){
  $('#gpCal').on('change',function(){S.year=null;load(this.value,false);});
  $('#gpCalNew').on('click',function(){calForm(null);});
  $('#gpCalEdit').on('click',function(){var c=cal();if(c)calForm(c);});
  $('#gpGen').on('click',genForm);
  $('#gpYears').on('click','.gp-yr',function(){S.year=$(this).data('y');render();});
  $('#gpCalInfo').on('click','[data-lp]',function(){lpForm($(this).data('lp'));});
  $('#gpPanel').on('click','#gpAdd',function(){periodForm(null);}).on('click','#gpYearEdit',yearForm).on('click','#gpSync',sync)
    .on('click','[data-lp]',function(){lpForm($(this).data('lp'));})
    .on('click','[data-open]',function(){quickOpen($(this).data('open'),$(this).data('code'),$(this).data('led'));})
    .on('click','[data-close]',function(){lpForm($(this).data('close'));})
    .on('click','[data-edit]',function(){var id=$(this).data('edit');periodForm(S.periods.find(function(p){return String(p.id)===String(id);}));});
  $('#gpMx,#gpMcancel').on('click',close);
  $('#gpMb').on('click',function(e){if(e.target===this)close();});
  $(document).on('keydown',function(e){if(e.key==='Escape'&&$('#gpMb').hasClass('open'))close();});
  $('#gpMok').on('click',function(){if(S.onOk)S.onOk();});
  $('#gpMc').on('click','[data-open]',function(){quickOpen($(this).data('open'),$(this).data('code'),$(this).data('led'));})
    .on('click','[data-close]',function(){var id=$(this).data('close');close();lpForm(id);})
    .on('click','.gp-lrow [data-lp]',function(){var id=$(this).data('lp');close();lpForm(id);});
  load(null,false);
});
})(apex.jQuery);