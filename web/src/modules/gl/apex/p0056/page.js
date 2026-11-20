apex.jQuery(function(){
(function($){
'use strict';
var ST={TB:'Trial balance',BS:'Balance sheet',IS:'Income statement',CF:'Cash flow'};
var SEC={ASSETS:'Assets',LIABILITIES:'Liabilities',EQUITY:'Equity',REVENUE:'Revenue',EXPENSES:'Expenses',
  OPERATING:'Operating activities',INVESTING:'Investing activities',FINANCING:'Financing activities',CASH_EQUIVALENT:'Cash and cash equivalents',TRIAL_BALANCE:'Accounts'};
var ORDER={TB:['TRIAL_BALANCE'],BS:['ASSETS','LIABILITIES','EQUITY'],IS:['REVENUE','EXPENSES'],CF:['OPERATING','INVESTING','FINANCING','CASH_EQUIVALENT']};
var S={d:null,st:'BS',chips:{},open:{},res:null,acc:{},q:null};
function esc(v){return apex.util.escapeHTML(v==null?'':String(v));}
function money(n){n=+n||0;var s=Math.abs(n).toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2});return n<-0.004?'('+s+')':s;}
function clean(m){return String(m||'').replace(/^ORA-\d+:\s*/,'').replace(/\nORA-[\s\S]*$/,'');}
function err(m){apex.message.clearErrors();apex.message.showErrors([{type:'error',location:'page',message:clean(m),unsafe:false}]);}
function call(name,data,ok,fail){apex.server.process(name,data,{dataType:'json'}).then(function(d){if(d&&d.ok===false){err(d.message||'The request failed.');if(fail)fail();return;}apex.message.clearErrors();ok(d||{});},
  function(x){err((x&&x.responseText)||'The request failed.');if(fail)fail();});}
function opt(v,t,sel){return '<option value="'+esc(v)+'"'+(String(v)===String(sel)?' selected':'')+'>'+esc(t)+'</option>';}
function seg(code){return (S.d.segments||[]).filter(function(s){return s[1]===code;})[0];}
function vname(code,v){var x=((S.d.values||{})[code]||[]).filter(function(r){return String(r[0]).toUpperCase()===String(v).toUpperCase();})[0];return x?x[1]:'';}

/* ---------- filters ---------- */
function filterSegs(){return (S.d.segments||[]).filter(function(s){return s[1]!=='NATURAL'&&((S.d.values||{})[s[1]]||[]).length>0;});}
function renderFilters(){var d=S.d;
  var h='<div class="fs-tabs" role="tablist">'+Object.keys(ST).map(function(k){return '<button type="button" role="tab" data-st="'+k+'" aria-selected="'+(k===S.st)+'">'+ST[k]+'</button>';}).join('')+'</div>'
   +'<div class="gs-g fs-g">'
   +'<div><label for="fsLed">Ledger</label><select id="fsLed">'+(d.ledgers||[]).map(function(l){return opt(l[0],l[1]+' · '+l[2],d.ledgerId);}).join('')+'</select></div>'
   +'<div><label for="fsLp">Period</label><select id="fsLp">'+(d.periods||[]).map(function(p){return opt(p[0],p[1]+(p[2]?'  ·  '+p[2].toLowerCase():''),d.lp);}).join('')+'</select></div>'
   +'<div><label for="fsCur">Currency</label><select id="fsCur">'+(d.currencies||[]).map(function(c){return opt(c,c,d.currency);}).join('')+'</select></div>'
   +'<div class="fs-mode"><label for="fsMode">Amounts</label><select id="fsMode"><option value="PTD">This period</option><option value="YTD">Year to date</option></select></div>'
   +'<div><label for="fsGrp">Columns by</label><select id="fsGrp"><option value="">No columns (total only)</option>'+filterSegs().map(function(s){return opt(s[1],s[2],'');}).join('')+'</select></div>'
   +'<div class="fs-rng"><label for="fsFrom">Accounts from / to</label><div><input id="fsFrom" class="code" placeholder="from" list="fsDlNat"><input id="fsTo" class="code" placeholder="to" list="fsDlNat"></div>'
   +'<datalist id="fsDlNat">'+(((d.values||{}).NATURAL)||[]).map(function(v){return '<option value="'+esc(v[0])+'" label="'+esc(v[1])+'"></option>';}).join('')+'</datalist></div>'
   +'</div>'
   +'<div class="fs-segs">'+filterSegs().map(function(s){
      return '<div class="fs-seg" data-seg="'+esc(s[1])+'"><label for="fsSeg'+s[0]+'">'+esc(s[2])+'</label><div class="fs-chips"><span class="fs-cl"></span>'
        +'<input id="fsSeg'+s[0]+'" list="fsDl'+s[0]+'" placeholder="All" autocomplete="off"></div>'
        +'<datalist id="fsDl'+s[0]+'">'+((d.values||{})[s[1]]||[]).map(function(v){return '<option value="'+esc(v[0])+'" label="'+esc(v[1]+(v[2]==='Y'?'':' (group)'))+'"></option>';}).join('')+'</datalist></div>';}).join('')+'</div>'
   +'<div class="gs-fb"><span class="fs-hint">Pick one or more values per segment. A group value includes every value under it. Empty means all.</span>'
   +'<div class="gp-act"><button type="button" class="gp-btn gp-btn--sm" id="fsReset">Clear filters</button><button type="button" class="gp-btn gp-btn--sm gp-btn--pri" id="fsGo">Run statement</button></div></div>';
  $('#fsF').html(h);S.chips={};drawChips();syncMode();
}
function syncMode(){$('#fsF .fs-mode').toggle(S.st==='IS');$('#fsF [data-st]').each(function(){$(this).attr('aria-selected',$(this).data('st')===S.st);});}
function drawChips(){$('#fsF .fs-seg').each(function(){var c=$(this).data('seg'),v=S.chips[c]||[];
  $(this).find('.fs-cl').html(v.map(function(x){var n=vname(c,x);return '<span class="fs-chip" title="'+esc(n)+'">'+esc(x)+(n?'<small>'+esc(n)+'</small>':'')+'<button type="button" data-rm="'+esc(x)+'" aria-label="Remove '+esc(x)+'">&times;</button></span>';}).join(''));
  $(this).toggleClass('on',v.length>0);});}
function addChip($in){var c=$in.closest('.fs-seg').data('seg'),v=$.trim($in.val()).toUpperCase();if(!v)return;
  var ok=((S.d.values||{})[c]||[]).some(function(r){return String(r[0]).toUpperCase()===v;});
  if(!ok){err('"'+v+'" is not a value of '+(seg(c)||[0,0,c])[2]+'.');return;}
  S.chips[c]=S.chips[c]||[];if(S.chips[c].indexOf(v)<0)S.chips[c].push(v);$in.val('');drawChips();}
function filterText(){return Object.keys(S.chips).filter(function(k){return S.chips[k].length;}).map(function(k){return k+'='+S.chips[k].join(',');}).join(';');}
function params(extra){return $.extend({st:S.st,led:+$('#fsLed').val(),lp:+$('#fsLp').val(),cur:$('#fsCur').val(),mode:S.st==='IS'?$('#fsMode').val():'PTD',
  filter:filterText(),group:$('#fsGrp').val(),level:'ACCOUNT',from:$.trim($('#fsFrom').val()),to:$.trim($('#fsTo').val())},extra||{});}

/* ---------- run + build ---------- */
function run(){var p=params();S.q=p;$('#fsGo').prop('disabled',true).text('Running...');
  call('FS_RUN',{x01:JSON.stringify(p)},function(d){$('#fsGo').prop('disabled',false).text('Run statement');S.res=d;S.open={};render();},function(){$('#fsGo').prop('disabled',false).text('Run statement');});}
function build(d){var rows=d.rows||[],q=S.q,grp=!!q.group,cols=[],seen={};
  rows.forEach(function(r){var g=grp?(r[5]||'-'):'_';if(!seen[g]){seen[g]=1;cols.push({k:g,n:grp?(r[6]||vname(q.group,g)||''):''});}});
  cols.sort(function(a,b){return a.k<b.k?-1:a.k>b.k?1:0;});
  var node={},secs={};
  function get(id,sec){var key=sec+'|'+id;if(node[key])return node[key];var a=S.acc[id]||{};
    var n=node[key]={id:id,sec:sec,code:a.code||'',name:a.name||'',parent:a.parent,kids:[],v:{},tb:[0,0,0,0,0,0],own:false};
    var pid=a.parent;if(pid&&S.acc[pid]){get(pid,sec).kids.push(n);}else{(secs[sec]=secs[sec]||[]).push(n);}return n;}
  rows.forEach(function(r){var sec=r[0],g=grp?(r[5]||'-'):'_',n;
    if(r[1]==null){n=node[sec+'|'+r[2]]=node[sec+'|'+r[2]]||{id:r[2],sec:sec,code:'',name:r[3],kids:[],v:{},tb:[0,0,0,0,0,0],own:true,synthetic:true};
      if((secs[sec]||[]).indexOf(n)<0)(secs[sec]=secs[sec]||[]).push(n);}
    else{n=get(r[1],sec);n.own=true;if(!n.code){n.code=r[2];n.name=r[3];}}
    n.v[g]=(n.v[g]||0)+(+r[16]||0);for(var i=0;i<6;i++)n.tb[i]+=(+r[10+i]||0);});
  function roll(n){n.kids.sort(function(a,b){return a.code<b.code?-1:a.code>b.code?1:0;});n.kids.forEach(function(k){roll(k);Object.keys(k.v).forEach(function(g){n.v[g]=(n.v[g]||0)+k.v[g];});for(var i=0;i<6;i++)n.tb[i]+=k.tb[i];});}
  Object.keys(secs).forEach(function(s){secs[s].forEach(roll);secs[s].sort(function(a,b){return a.synthetic?1:b.synthetic?-1:(a.code<b.code?-1:a.code>b.code?1:0);});});
  var tot={};Object.keys(secs).forEach(function(s){var t={v:{},tb:[0,0,0,0,0,0]};secs[s].forEach(function(n){Object.keys(n.v).forEach(function(g){t.v[g]=(t.v[g]||0)+n.v[g];});for(var i=0;i<6;i++)t.tb[i]+=n.tb[i];});tot[s]=t;});
  return {cols:cols,secs:secs,tot:tot,grp:grp};}
function sum(v){return Object.keys(v).reduce(function(a,k){return a+v[k];},0);}
function sv(t,s,g){return t[s]?(g==null?sum(t[s].v):(t[s].v[g]||0)):0;}

function render(){var d=S.res,q=S.q;if(!d)return;var b=build(d),st=q.st,tbMode=st==='TB'&&!b.grp;S.b=b;
  var per=(S.d.periods||[]).filter(function(p){return String(p[0])===String(q.lp);})[0]||[];
  var fl=Object.keys(S.chips).filter(function(k){return S.chips[k].length;}).map(function(k){return '<span><b>'+esc((seg(k)||[0,0,k])[2])+'</b> '+S.chips[k].map(esc).join(', ')+'</span>';});
  if(q.from||q.to)fl.push('<span><b>Accounts</b> '+esc(q.from||'…')+' – '+esc(q.to||'…')+'</span>');
  $('#fsTitle').html(esc(ST[st])+'<small>'+esc(per[1]||'')+(st==='IS'?' · '+(q.mode==='YTD'?'year to date':'this period'):'')+' · '+esc(q.cur)+'</small>');
  $('#fsScope').html(fl.length?'Filtered by '+fl.join(''):'<span>All segment values</span>');
  kpis(b,st);
  var cols=b.cols,head;
  if(tbMode)head='<tr><th>Account</th><th class="num">Opening</th><th class="num">Debits</th><th class="num">Credits</th><th class="num">Closing</th></tr>';
  else head='<tr><th>Account</th>'+(b.grp?cols.map(function(c){return '<th class="num" title="'+esc(c.n)+'">'+esc(c.k)+(c.n?'<small>'+esc(c.n)+'</small>':'')+'</th>';}).join('')+'<th class="num tot">Total</th>':'<th class="num">'+(st==='TB'?'Closing (Dr +)':'Amount')+'</th>')+'</tr>';
  var ncol=tbMode?5:(b.grp?cols.length+2:2),body='',any=false;
  function cells(v,tb,cls){if(tbMode){var o=tb[0]-tb[1],c=tb[4]-tb[5];return '<td class="num">'+money(o)+'</td><td class="num">'+money(tb[2])+'</td><td class="num">'+money(tb[3])+'</td><td class="num '+(cls||'')+'">'+money(c)+'</td>';}
    return (b.grp?cols.map(function(c){return '<td class="num">'+money(v[c.k]||0)+'</td>';}).join(''):'')+'<td class="num tot '+(cls||'')+'">'+money(sum(v))+'</td>';}
  function line(n,lvl){var has=n.kids.length>0,key=n.sec+'|'+n.id,open=S.open[key]!==false;any=true;
    var h='<tr class="fs-r lv'+Math.min(lvl,4)+(has?' par':'')+(n.synthetic?' syn':'')+'" data-key="'+esc(key)+'"><td><div class="fs-a" style="padding-inline-start:'+(lvl*20)+'px">'
      +(has?'<button type="button" class="fs-tw" aria-expanded="'+open+'" aria-label="Show or hide">'+(open?'▾':'▸')+'</button>':'<span class="fs-tw0"></span>')
      +(n.code?'<code>'+esc(n.code)+'</code>':'')+'<span class="nm">'+esc(n.name)+'</span>'
      +(!has&&!n.synthetic?'<button type="button" class="fs-dr" data-acc="'+esc(n.id)+'" title="See the account combinations behind this amount">combinations</button>':'')+'</div></td>'+cells(n.v,n.tb)+'</tr>';
    if(has&&open)n.kids.forEach(function(k){h+=line(k,lvl+1);});return h;}
  (ORDER[st]||[]).concat(Object.keys(b.secs).filter(function(s){return (ORDER[st]||[]).indexOf(s)<0;})).forEach(function(s){
    if(!b.secs[s])return;
    body+='<tr class="fs-sec"><td colspan="'+ncol+'">'+esc(SEC[s]||s)+'</td></tr>';
    b.secs[s].forEach(function(n){body+=line(n,0);});
    if(st!=='TB')body+='<tr class="fs-sub"><td>Total '+esc((SEC[s]||s).toLowerCase())+'</td>'+cells(b.tot[s].v,b.tot[s].tb)+'</tr>';});
  body+=grand(b,st,cells);
  $('#fsHead').html(head);
  $('#fsRows').html(any?body:'<tr><td colspan="'+ncol+'"><div class="gp-empty"><b>Nothing to show</b>No balances match this period, currency and filter.</div></td></tr>');
  $('#fsOut').prop('hidden',false);$('#fsMore').toggle(!!d.more);
}
function grand(b,st,cells){var one=function(lbl,fn,k){var v={},tb=[0,0,0,0,0,0];b.cols.forEach(function(c){v[c.k]=fn(c.k);});return '<tr class="fs-tot'+(k?' '+k:'')+'"><td>'+lbl+'</td>'+cells(v,tb)+'</tr>';};
  if(st==='BS')return one('Total liabilities and equity',function(k){return sv(b.tot,'LIABILITIES',k)+sv(b.tot,'EQUITY',k);})
    +one('Difference (assets − liabilities − equity)',function(k){return sv(b.tot,'ASSETS',k)-sv(b.tot,'LIABILITIES',k)-sv(b.tot,'EQUITY',k);},'chk');
  if(st==='IS')return one('Net profit',function(k){return sv(b.tot,'REVENUE',k)-sv(b.tot,'EXPENSES',k);},'net');
  if(st==='CF')return one('Net cash flow (operating + investing + financing)',function(k){return sv(b.tot,'OPERATING',k)+sv(b.tot,'INVESTING',k)+sv(b.tot,'FINANCING',k);},'net')
    +one('Difference against the change in cash',function(k){return sv(b.tot,'OPERATING',k)+sv(b.tot,'INVESTING',k)+sv(b.tot,'FINANCING',k)-sv(b.tot,'CASH_EQUIVALENT',k);},'chk');
  if(st==='TB'){var t=b.tot.TRIAL_BALANCE||{v:{},tb:[0,0,0,0,0,0]};return '<tr class="fs-tot net"><td>Total</td>'+cells(t.v,t.tb)+'</tr>';}
  return '';}
function kpis(b,st){var t=b.tot,k=[];
  function x(l,v,c){k.push('<div'+(c?' class="'+c+'"':'')+'><small>'+l+'</small><b>'+money(v)+'</b></div>');}
  function ok(v){return Math.abs(v)<0.005;}
  if(st==='BS'){var a=sv(t,'ASSETS'),l=sv(t,'LIABILITIES'),e=sv(t,'EQUITY'),df=a-l-e;x('Total assets',a);x('Total liabilities',l);x('Total equity',e);
    k.push('<div class="'+(ok(df)?'ok':'bad')+'"><small>Check</small><b>'+(ok(df)?'Balanced':'Out by '+money(df))+'</b></div>');}
  else if(st==='IS'){var r=sv(t,'REVENUE'),ex=sv(t,'EXPENSES'),np=r-ex;x('Revenue',r);x('Expenses',ex);x('Net profit',np,np<0?'bad':'ok');
    k.push('<div><small>Net margin</small><b>'+(Math.abs(r)>0.004?(np/r*100).toFixed(1)+'%':'–')+'</b></div>');}
  else if(st==='CF'){var o=sv(t,'OPERATING'),i=sv(t,'INVESTING'),f=sv(t,'FINANCING'),c=sv(t,'CASH_EQUIVALENT');x('Operating',o);x('Investing',i);x('Financing',f);
    k.push('<div class="'+(ok(o+i+f-c)?'ok':'bad')+'"><small>Change in cash</small><b>'+money(c)+'</b></div>');}
  else{var dr=0,cr=0;
    (S.res.rows||[]).forEach(function(r){var c=(+r[14]||0)-(+r[15]||0);if(c>0)dr+=c;else cr-=c;});
    x('Debit balances',dr);x('Credit balances',cr);k.push('<div class="'+(ok(dr-cr)?'ok':'bad')+'"><small>Check</small><b>'+(ok(dr-cr)?'Balanced':'Out by '+money(dr-cr))+'</b></div>');}
  $('#fsKpi').html(k.join(''));}

/* ---------- drill-down: the combinations behind one account ---------- */
function drill(acc){var a=S.acc[acc]||{},p=$.extend({},S.q,{level:'COMBINATION',acc:+acc});
  $('#fsDrT').text((a.code?a.code+' ':'')+(a.name||''));$('#fsDrB').html('<div class="gp-empty">Loading...</div>');$('#fsDr').addClass('open').attr('aria-hidden','false');
  call('FS_RUN',{x01:JSON.stringify(p)},function(d){var m={},o=[];(d.rows||[]).forEach(function(r){if(r[1]==null)return;var k=r[8]||'';if(!m[k]){m[k]={c:k,v:0,g:{}};o.push(m[k]);}m[k].v+=+r[16]||0;if(p.group)m[k].g[r[5]||'-']=1;});
    o.sort(function(x,y){return x.c<y.c?-1:1;});var t=o.reduce(function(s,x){return s+x.v;},0);
    $('#fsDrB').html(o.length?'<table class="gp-t"><thead><tr><th>Account combination</th><th class="num">Amount</th></tr></thead><tbody>'
      +o.map(function(x){return '<tr><td><code>'+esc(x.c)+'</code></td><td class="num">'+money(x.v)+'</td></tr>';}).join('')
      +'<tr class="fs-tot"><td>Total</td><td class="num">'+money(t)+'</td></tr></tbody></table>':'<div class="gp-empty">No combinations.</div>');});}
function closeDrill(){$('#fsDr').removeClass('open').attr('aria-hidden','true');}

/* ---------- export ---------- */
function csv(){var rows=[];$('#fsTbl tr').each(function(){if($(this).find('td[colspan]').length&&!$(this).hasClass('fs-sec'))return;
  rows.push($(this).find('th,td').map(function(){var c=$(this).clone();c.find('button.fs-dr,button.fs-tw,small').remove();c.find('code').append(' ');var t=$.trim(c.text().replace(/\s+/g,' '));
    var m=/^\(([\d,.]+)\)$/.exec(t);if(m)t='-'+m[1];if(/^-?[\d,]+\.\d\d$/.test(t))t=t.replace(/,/g,'');return '"'+t.replace(/"/g,'""')+'"';}).get().join(','));});
  var blob=new Blob(['﻿'+rows.join('\r\n')],{type:'text/csv;charset=utf-8'}),a=document.createElement('a');
  a.href=URL.createObjectURL(blob);a.download=(ST[S.q.st]||'statement').replace(/ /g,'-').toLowerCase()+'.csv';document.body.appendChild(a);a.click();setTimeout(function(){URL.revokeObjectURL(a.href);a.remove();},500);}

/* ---------- init + events ---------- */
function load(led){call('FS_INIT',{x01:led||''},function(d){S.d=d;S.acc={};(d.accounts||[]).forEach(function(a){S.acc[a[0]]={code:a[1],name:a[2],parent:a[3],summary:a[4]};});
  renderFilters();});}
$(document).on('click','#fsF [data-st]',function(){S.st=$(this).data('st');syncMode();if(S.res)run();});
$(document).on('change','#fsLed',function(){load($(this).val());});
$(document).on('change','#fsF .fs-seg input',function(){addChip($(this));});
$(document).on('keydown','#fsF .fs-seg input',function(e){if(e.key==='Enter'){e.preventDefault();if($.trim($(this).val()))addChip($(this));else run();}
  else if(e.key==='Backspace'&&!$(this).val()){var c=$(this).closest('.fs-seg').data('seg');if((S.chips[c]||[]).length){S.chips[c].pop();drawChips();}}});
$(document).on('keydown','#fsFrom,#fsTo',function(e){if(e.key==='Enter'){e.preventDefault();run();}});
$(document).on('click','#fsF [data-rm]',function(){var c=$(this).closest('.fs-seg').data('seg'),v=String($(this).data('rm'));S.chips[c]=(S.chips[c]||[]).filter(function(x){return x!==v;});drawChips();});
$(document).on('click','#fsReset',function(){S.chips={};drawChips();$('#fsFrom,#fsTo').val('');$('#fsGrp').val('');});
$(document).on('click','#fsGo',run);
$(document).on('click','#fsRows .fs-tw',function(){var k=$(this).closest('tr').data('key'),o=$(this).attr('aria-expanded')==='true';S.open[k]=!o;render();});
$(document).on('click','#fsRows .fs-dr',function(){drill($(this).data('acc'));});
$(document).on('click','#fsExpand',function(){var b=S.b;if(!b)return;var all=function(n){S.open[n.sec+'|'+n.id]=true;n.kids.forEach(all);};Object.keys(b.secs).forEach(function(s){b.secs[s].forEach(all);});render();});
$(document).on('click','#fsCollapse',function(){var b=S.b;if(!b)return;var all=function(n){S.open[n.sec+'|'+n.id]=false;n.kids.forEach(all);};Object.keys(b.secs).forEach(function(s){b.secs[s].forEach(all);});render();});
$(document).on('click','#fsCsv',function(){if(S.res)csv();});
$(document).on('click','#fsPrint',function(){window.print();});
$(document).on('click','#fsDr [data-close], #fsDr',function(e){if(e.target===this||$(e.target).closest('[data-close]').length)closeDrill();});
$(document).on('keydown',function(e){if(e.key==='Escape')closeDrill();});
window.FinovaFS={state:S,run:run,load:load};
$(function(){load();});
})(apex.jQuery);
});
