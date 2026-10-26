(function($){
'use strict';
var S={coaId:null,coas:[],segs:[],acc:[],byId:{},kids:{},types:[],lists:{},sel:null,col:{},gcol:{},q:'',flt:'ALL',onOk:null};
var I={chev:'<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m6 9 6 6 6-6"/></svg>',folder:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/></svg>',leaf:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="6" y="4" width="12" height="16" rx="2"/><path d="M9 9h6M9 13h6"/></svg>',plus:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 5v14M5 12h14"/></svg>',edit:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 20h9"/><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z"/></svg>',tree:'<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="3" width="6" height="5" rx="1"/><rect x="15" y="10" width="6" height="5" rx="1"/><rect x="15" y="17" width="6" height="4" rx="1"/><path d="M6 8v9a2 2 0 0 0 2 2h7M6 12h9"/></svg>'};
var GROUPS=[{k:'ASSET',l:'Assets',c:'#E87722'},{k:'LIABILITY',l:'Liabilities',c:'#3567D6'},{k:'EQUITY',l:'Equity',c:'#7B61C9'},{k:'INCOME',l:'Income',c:'#14734F'},{k:'EXPENSE',l:'Expenses',c:'#D14B4B'}];
var FLAGS=[['control','Control account'],['recon','Reconciliation'],['manual','Manual journals'],['subledger','Sub-ledger posting'],['reqBranch','Branch required'],['reqCc','Cost centre required'],
  ['reqBl','Business line required'],['reval','Currency revaluation'],['retained','Retained earnings']];
function esc(v){return apex.util.escapeHTML(v==null?'':String(v));}
function lbl(v){v=String(v||'').replace(/_/g,' ').toLowerCase();return v.replace(/\b\w/g,function(c){return c.toUpperCase();});}
function yn(v){return String(v||'').trim()==='Y';}
function U(v){return String(v||'').trim().toUpperCase();}
function bucket(t){t=U(t).replace(/[_-]+/g,' ');if(t.indexOf('ASSET')>=0)return 'ASSET';if(t.indexOf('LIAB')>=0)return 'LIABILITY';if(t.indexOf('EQUIT')>=0)return 'EQUITY';
  if(t.indexOf('INCOME')>=0||t.indexOf('REVENUE')>=0)return 'INCOME';if(t.indexOf('EXPENSE')>=0)return 'EXPENSE';return 'ASSET';}
function grp(k){return GROUPS.find(function(g){return g.k===k;});}
function sorter(a,b){var x=+a.sortOrder,y=+b.sortOrder;if(!isNaN(x)&&!isNaN(y)&&x!==y)return x-y;return String(a.code).localeCompare(String(b.code),undefined,{numeric:true});}
function fd(d){if(!d)return '-';var p=d.split('-');var m=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];return p[2]+' '+m[+p[1]-1]+' '+p[0];}
function coa(){return S.coas.find(function(c){return String(c.id)===String(S.coaId);});}
function accSeg(){var c=coa();if(!c)return null;return S.segs.find(function(s){return +s.num===+c.accountSeg;});}
function segMismatch(){ // the structure must point its account segment at a segment of type ACCOUNT
  var s=accSeg(),t=S.segs.find(function(x){return U(x.type)==='ACCOUNT';});
  if(s&&U(s.type)==='ACCOUNT')return null;
  return 'The account structure says the account segment is '+(s?'segment '+s.num+' ('+s.nameEn+', type '+lbl(s.type)+')':'not set')
    +(t?', but segment '+t.num+' ('+t.nameEn+') is the Account segment':'')+'. Fix it on the Account segments page (Edit structure \u2192 Account segment) before adding accounts.';}
function call(name,data,ok){apex.server.process(name,data,{dataType:'json'}).then(function(d){if(d&& d.ok===false){showErr(d.message||'The request failed.');return;}ok(d||{});},
  function(x){showErr((x&& x.responseText)||'The request failed.');});}
function showErr(m){m=String(m||'').replace(/^ORA-\d+:\s*/,'').replace(/\nORA-[\s\S]*$/,'');
  if($('#gpMb').hasClass('open')){$('#gpErr').text(m).addClass('show');$('#gpMok').prop('disabled',false);}else{apex.message.clearErrors();apex.message.showErrors([{type:'error',location:'page',message:m,unsafe:false}]);}}
/* ---------------------------------------------------------------- data */
function load(coaId,keep){
  call('GL_TREE_LOAD',{x01:coaId||''},function(d){
    S.coaId=d.coaId;S.coas=d.structures||[];S.segs=d.segments||[];S.acc=d.accounts||[];S.types=d.types||[];S.lists=d.lists||{};
    S.byId={};S.kids={};S.acc.forEach(function(a){S.byId[a.id]=a;});
    S.acc.forEach(function(a){var p=a.parentId&& S.byId[a.parentId]?a.parentId:'';(S.kids[p]=S.kids[p]||[]).push(a);});
    Object.keys(S.kids).forEach(function(k){S.kids[k].sort(sorter);});
    if(keep&& S.sel&&!S.byId[S.sel])S.sel=null;if(!keep)S.sel=null;
    render();
  });
}
function kids(id){return S.kids[id||'']||[];}
function roots(g){return kids('').filter(function(a){return bucket(a.type)===g;});}
function path(a){var p=[],c=a,n=0;while(c&&n++<40){p.unshift(c);c=c.parentId?S.byId[c.parentId]:null;}return p;}
function match(a){
  if(S.flt==='POST'&&!yn(a.posting))return false;if(S.flt==='SUM'&&!yn(a.summary))return false;if(S.flt==='OFF'&&yn(a.active))return false;if(S.flt==='NL'&& a.segCode)return false;
  if(S.q&&(a.code+' '+a.name+' '+(a.nameAr||'')).toUpperCase().indexOf(S.q)<0)return false;return true;}
function hl(t){t=esc(t);if(!S.q)return t;var i=t.toUpperCase().indexOf(esc(S.q).toUpperCase());if(i<0)return t;var L=esc(S.q).length;return t.slice(0,i)+'<mark>'+t.slice(i,i+L)+'</mark>'+t.slice(i+L);}
/* ---------------------------------------------------------------- render */
function render(){
  $('#ctCoa').html(S.coas.map(function(c){return '<option value="'+c.id+'"'+(String(c.id)===String(S.coaId)?' selected':'')+'>'+esc(c.code+' - '+c.nameEn)+'</option>';}).join('')||'<option>No structure</option>');
  var n=S.acc.length,post=0,sum=0,off=0,nl=0;S.acc.forEach(function(a){if(yn(a.posting))post++;if(yn(a.summary))sum++;if(!yn(a.active))off++;if(!a.segCode)nl++;});
  var s=accSeg();
  $('#ctKpis').html('<div class="ct-kpi"><b>'+n+'</b><span>Accounts</span></div><div class="ct-kpi"><b>'+post+'</b><span>Posting</span></div><div class="ct-kpi"><b>'+sum+'</b><span>Summary</span></div>'
    +'<div class="ct-kpi"><b>'+off+'</b><span>Inactive</span></div><div class="ct-kpi'+(nl?' warn':'')+'"><b>'+nl+'</b><span>Not linked to a segment value</span></div>'
    +'<div class="ct-kpi'+(segMismatch()?' bad':'')+'" title="'+esc(segMismatch()||'')+'"><b style="font-size:14px">'+(segMismatch()?'Check structure':(s?esc('Segment '+s.num+' \u00b7 '+s.nameEn):'-'))+'</b><span>'+(s?esc(s.minLen+(s.maxLen!=s.minLen?'–'+s.maxLen:'')+' chars · '+lbl(s.format)):'Account segment')+'</span></div>');
  $('#ctChips').html([['ALL','All',n],['POST','Posting',post],['SUM','Summary',sum],['OFF','Inactive',off],['NL','Not linked',nl]].map(function(c){
    return '<button type="button" class="ct-chip'+(S.flt===c[0]?' on':'')+'" data-f="'+c[0]+'">'+c[1]+' <b>'+c[2]+'</b></button>';}).join(''));
  renderTree();renderDet();
}
function visibleSet(){ // accounts that match, plus their ancestors (so the tree keeps its shape while filtering)
  if(!S.q&& S.flt==='ALL')return null;var set={};S.acc.forEach(function(a){if(match(a))path(a).forEach(function(p){set[p.id]=1;});});return set;}
function renderTree(){
  var vs=visibleSet(),h='',rows=0;
  GROUPS.forEach(function(g){var list=roots(g.k).filter(function(a){return !vs||vs[a.id];});var cnt=S.acc.filter(function(a){return bucket(a.type)===g.k;}).length;
    if(vs&&!list.length)return;
    h+='<div class="ct-grp'+(S.gcol[g.k]?' col':'')+'" data-g="'+g.k+'"><span class="gc">'+I.chev+'</span><span class="gi" style="background:'+g.c+'">'+I.tree+'</span><b>'+g.l+'</b><small>'+cnt+' accounts</small>'
      +'<button type="button" class="ga" data-ag="'+g.k+'" title="Add a top-level '+g.l.toLowerCase()+' account" aria-label="Add '+g.l+' account">'+I.plus+'</button></div>';
    if(S.gcol[g.k])return;
    (function walk(arr,guides){arr.forEach(function(a,i){var last=i===arr.length-1,ch=kids(a.id).filter(function(k){return !vs||vs[k.id];}),open=!S.col[a.id]||(vs&&!S.col[a.id]);
      var gl=guides.map(function(more){return '<span class="ct-g'+(more?' v':'')+'"></span>';}).join('')+(guides.length||true?'<span class="ct-g '+(last?'l':'t more')+'"></span>':'');
      var dim=vs&&!match(a);rows++;
      h+='<div class="ct-r'+(yn(a.summary)?' sum':'')+(yn(a.active)?'':' off')+(String(S.sel)===String(a.id)?' sel':'')+(ch.length&&!open?' col':'')+'" data-id="'+a.id+'" role="treeitem" aria-level="'+(guides.length+1)+'"'
        +(ch.length?' aria-expanded="'+open+'"':'')+' aria-selected="'+(String(S.sel)===String(a.id))+'" style="'+(dim?'opacity:.55':'')+'">'
        +gl+(ch.length?'<span class="ct-tg" data-t="'+a.id+'">'+I.chev+'</span>':'<span class="ct-tg"></span>')
        +'<span class="ct-ic '+(yn(a.summary)||ch.length?'sum':'post')+'">'+(yn(a.summary)||ch.length?I.folder:I.leaf)+'</span>'
        +'<span class="ct-cd">'+hl(a.code)+'</span><span class="ct-nm" title="'+esc(a.name)+'">'+hl(a.name)+'</span>'
        +(ch.length?'<span class="ct-n">'+ch.length+'</span>':'')
        +'<span class="ct-bd">'+(yn(a.control)?'<span class="ct-b ctl" title="Control account">CTL</span>':'')+(!a.segCode?'<span class="ct-b nl" title="Not linked to an account segment value">!</span>':'')
        +'<span class="ct-b '+(U(a.normalBalance).charAt(0)==='C'?'cr':'dr')+'" title="Normal balance">'+esc(U(a.normalBalance).charAt(0)==='C'?'CR':'DR')+'</span></span></div>';
      if(ch.length&&open)walk(ch,guides.concat([!last]));});})(list,[]);
  });
  if(!h)h='<div class="ct-empty">'+(S.acc.length?'No account matches.':'No accounts yet. Use <b>Add account</b>.')+'</div>';
  $('#ctTree').html(h);
}
function renderDet(){
  var a=S.sel&& S.byId[S.sel];
  if(!a){$('#ctDet').html('<div class="cd-empty"><div>'+'<svg width="44" height="44" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="6" height="5" rx="1"/><rect x="15" y="10" width="6" height="5" rx="1"/><rect x="15" y="17" width="6" height="4" rx="1"/><path d="M6 8v9a2 2 0 0 0 2 2h7M6 12h9"/></svg>'
    +'<div><b style="color:#0B2533">Select an account</b><br>Use the tree, the search or the arrow keys.</div></div></div>');return;}
  var g=grp(bucket(a.type)),p=path(a),ch=kids(a.id),c=coa(),s=accSeg();
  var h='<div class="cd-head" style="--gc:'+g.c+'"><div class="cd-crumb"><span>'+g.l+'</span>'+p.slice(0,-1).map(function(x){return ' › <a data-go="'+x.id+'">'+esc(x.code+' '+x.name)+'</a>';}).join('')+'</div>'
    +'<div class="cd-title"><div><h2><code>'+esc(a.code)+'</code>'+esc(a.name)+'</h2>'+(a.nameAr?'<span class="cd-ar">'+esc(a.nameAr)+'</span>':'')
    +'<div class="cd-pills">'+(yn(a.active)?'<span class="gp-pill ok">Active</span>':'<span class="gp-pill off">Inactive</span>')
    +'<span class="gp-tag">'+(yn(a.summary)?'Summary':'Posting')+' account</span><span class="gp-tag">Normal balance: '+esc(lbl(a.normalBalance))+'</span>'+(yn(a.control)?'<span class="gp-tag">Control</span>':'')+'</div></div>'
    +'<div class="cd-acts"><button type="button" class="gp-btn" id="cdChild">'+I.plus+'<span>Add child</span></button><button type="button" class="gp-btn gp-btn--pri" id="cdEdit">'+I.edit+'<span>Edit</span></button></div></div></div>';
  h+='<div class="cd-body"><div class="cd-stats"><div class="cd-stat"><small>Level</small><b>'+p.length+'</b></div><div class="cd-stat"><small>Child accounts</small><b>'+ch.length+'</b></div>'
    +'<div class="cd-stat"><small>Combinations</small><b>'+esc(a.combos||0)+'</b>'+(a.combos?'<small>'+esc(a.combosOn||0)+' enabled</small>':'')+'</div>'
    +(a.segCode?'<div class="cd-stat"><small>Account segment value</small><b>'+esc(a.segCode)+'</b><small>'+esc(s?('Segment '+s.num+' · '+s.nameEn):'')+'</small></div>'
      :'<div class="cd-stat warn"><small>Account segment value</small><b>Not linked</b><small>Edit and save to link it</small></div>')+'</div>';
  h+='<div class="cd-sec"><h4>Classification</h4><div class="cd-grid"><div><small>Account type</small><b>'+esc(lbl(a.type))+'</b></div><div><small>Normal balance</small><b>'+esc(lbl(a.normalBalance))+'</b></div>'
    +'<div><small>Financial statement class</small><b>'+esc(lbl(a.fsClass)||'-')+'</b></div><div><small>Cash flow class</small><b>'+esc(lbl(a.cashFlowClass)||'-')+'</b></div>'
    +'<div><small>Parent account</small><b>'+(a.parentId&& S.byId[a.parentId]?esc(S.byId[a.parentId].code+' - '+S.byId[a.parentId].name):'Top level')+'</b></div><div><small>Effective</small><b>'+fd(a.start)+(a.end?' – '+fd(a.end):' onwards')+'</b></div>'
    +(a.description?'<div style="grid-column:1/-1;border-right:0"><small>Description</small><b>'+esc(a.description)+'</b></div>':'')+'</div></div>';
  h+='<div class="cd-sec"><h4>Balances by segment</h4><div id="cdBal"><div class="ct-empty">Loading balances\u2026</div></div></div>';
  h+='<div class="cd-sec"><h4>Controls</h4><div class="cd-flags">'+FLAGS.map(function(f){var on=yn(a[f[0]]);return '<div class="cd-flag'+(on?' on':'')+'"><i>'+(on?'✓':'–')+'</i>'+f[1]+'</div>';}).join('')+'</div></div>';
  if(ch.length)h+='<div class="cd-sec"><h4>Child accounts</h4><div class="cd-kids">'+ch.slice(0,24).map(function(k){return '<a data-go="'+k.id+'"><code>'+esc(k.code)+'</code>'+esc(k.name)+'</a>';}).join('')+(ch.length>24?'<span class="gp-tag">+'+(ch.length-24)+' more</span>':'')+'</div></div>';
  h+='<div class="cd-sec"><h4>Audit</h4><div class="cd-grid"><div><small>Created</small><b>'+esc(a.createdBy||'-')+'</b><small>'+esc(a.createdDt||'')+'</small></div><div><small>Last updated</small><b>'+esc(a.updatedBy||'-')+'</b><small>'+esc(a.updatedDt||'')+'</small></div></div></div></div>';
  $('#ctDet').html(h);
  loadBal(a.id);
}

/* ---------------------------------------------------------------- balances by segment */
function num(n){return (+n||0).toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2});}
function net(n){n=Math.round((+n||0)*100)/100;return n===0?'0.00':num(Math.abs(n))+(n>0?' Dr':' Cr');}
function curList(d){var m={};(d.combos||[]).forEach(function(c){(c.bals||[]).forEach(function(b){m[b.cur]=1;});});if(d.ledgerCurrency)m[d.ledgerCurrency]=1;return Object.keys(m).sort();}
function loadBal(accId,led,lp){S.bal=S.bal||{group:'COMB',nz:false};var B=S.bal;
  apex.server.process('GL_ACC_BAL',{x01:accId,x02:led||B.led||'',x03:lp||''},{dataType:'json'}).then(function(d){
    if(String(S.sel)!==String(accId))return;
    if(d&& d.ok===false){$('#cdBal').html('<div class="ct-empty">'+esc(d.message||'Balances could not be loaded.')+'</div>');return;}
    B.acc=accId;B.d=d;B.led=d.ledgerId;B.lp=d.lpId;var cs=curList(d);
    if(!B.cur||cs.indexOf(B.cur)<0)B.cur=(d.ledgerCurrency&& cs.indexOf(d.ledgerCurrency)>=0)?d.ledgerCurrency:(cs[0]||'');renderBal();
  },function(x){$('#cdBal').html('<div class="ct-empty">Balances could not be loaded.</div>');});}
function renderBal(){var B=S.bal,d=B&& B.d;if(!d)return;
  var segs=(d.segments||[]).slice().sort(function(a,b){return a.num-b.num;}),cs=curList(d);
  var h='<div class="cb-bar"><label>Ledger<select id="cbLed">'+(d.ledgers||[]).map(function(l){return '<option value="'+l.id+'"'+(String(l.id)===String(d.ledgerId)?' selected':'')+'>'+esc(l.code)+'</option>';}).join('')+'</select></label>'
    +'<label>Period<select id="cbLp">'+(d.periods||[]).map(function(p){return '<option value="'+p.lp+'"'+(String(p.lp)===String(d.lpId)?' selected':'')+'>'+esc(p.code+' · '+lbl(p.status))+'</option>';}).join('')+'</select></label>'
    +(cs.length>1?'<label>Currency<select id="cbCur">'+cs.map(function(c){return '<option'+(c===B.cur?' selected':'')+'>'+esc(c)+'</option>';}).join('')+'</select></label>':(B.cur?'<span class="gp-tag">'+esc(B.cur)+'</span>':''))+'</div>';
  if(!d.ledgers||!d.ledgers.length){$('#cdBal').html('<div class="ct-empty">No ledger uses this account structure.</div>');return;}
  var hasAccType=segs.some(function(x){return U(x.type)==='ACCOUNT';});
  var opts=[['COMB','Combination']].concat(segs.filter(function(x){return hasAccType?U(x.type)!=='ACCOUNT':+x.num!==+d.accountSeg;}).map(function(x){return [String(x.num),x.name];}));
  if(d.accounts>1)opts.splice(1,0,['ACC','Account']);
  h+='<div class="cb-grp"><span>Break down by</span>'+opts.map(function(o){return '<button type="button" class="ct-chip'+(B.group===o[0]?' on':'')+'" data-gb="'+o[0]+'">'+esc(o[1])+'</button>';}).join('')
    +'<label class="cb-nz"><input type="checkbox" id="cbNz"'+(B.nz?' checked':'')+'> Hide zero lines</label></div>';
  var combos=d.combos||[];
  if(!combos.length){$('#cdBal').html(h+'<div class="ct-empty" style="padding:22px">No account combination uses '+(d.accounts>1?'this account or its child accounts':'this account')+' yet. A combination is created the first time a journal line posts to it.</div>');return;}
  var groups={},order=[];
  combos.forEach(function(c){var b=(c.bals||[]).find(function(x){return x.cur===B.cur;})||{};
    var v={op:(+b.odr||0)-(+b.ocr||0),dr:+b.pdr||0,cr:+b.pcr||0,cl:(+b.cdr||0)-(+b.ccr||0)},key,lab,sub='';
    if(B.group==='COMB'){key=c.concat;lab='<code class="cb-cc">'+esc(c.concat)+'</code>';sub=(c.enabled==='Y'?'':'<span class="gp-pill off">Disabled</span>');}
    else if(B.group==='ACC'){key=c.accCode;lab='<code class="cb-cc">'+esc(c.accCode)+'</code> '+esc(c.accName||'');}
    else{var sg=(c.segs||[]).find(function(x){return String(x.num)===B.group;})||{};key=sg.code||'–';lab='<code class="cb-cc">'+esc(sg.code||'–')+'</code> '+esc(sg.name||'');}
    if(!groups[key]){groups[key]={lab:lab,sub:sub,n:0,op:0,dr:0,cr:0,cl:0};order.push(key);}
    var g=groups[key];g.n++;g.op+=v.op;g.dr+=v.dr;g.cr+=v.cr;g.cl+=v.cl;});
  order.sort(function(a,b){return String(a).localeCompare(String(b),undefined,{numeric:true});});
  var rows=order.map(function(k){return groups[k];}).filter(function(g){return !B.nz||Math.abs(g.op)+g.dr+g.cr+Math.abs(g.cl)>0.004;});
  var T={n:0,op:0,dr:0,cr:0,cl:0};rows.forEach(function(g){T.n+=g.n;T.op+=g.op;T.dr+=g.dr;T.cr+=g.cr;T.cl+=g.cl;});
  var gname=B.group==='COMB'?'Combination':B.group==='ACC'?'Account':((segs.find(function(x){return String(x.num)===B.group;})||{}).name||'Segment');
  h+='<div class="gp-tw"><table class="gp-t cb-t"><thead><tr><th>'+esc(gname)+'</th>'+(B.group==='COMB'?'':'<th class="r">Comb.</th>')+'<th class="r">Opening</th><th class="r">Debit</th><th class="r">Credit</th><th class="r">Closing</th></tr></thead><tbody>'
    +(rows.length?rows.map(function(g){return '<tr><td>'+g.lab+(g.sub?' '+g.sub:'')+'</td>'+(B.group==='COMB'?'':'<td class="r num">'+g.n+'</td>')
      +'<td class="r num">'+net(g.op)+'</td><td class="r num">'+num(g.dr)+'</td><td class="r num">'+num(g.cr)+'</td><td class="r num"><b>'+net(g.cl)+'</b></td></tr>';}).join('')
      :'<tr><td colspan="6" class="cb-none">All lines are zero for this period.</td></tr>')
    +'</tbody><tfoot><tr><td>Total'+(d.accounts>1?' · '+d.accounts+' accounts':'')+'</td>'+(B.group==='COMB'?'':'<td class="r num">'+T.n+'</td>')
    +'<td class="r num">'+net(T.op)+'</td><td class="r num">'+num(T.dr)+'</td><td class="r num">'+num(T.cr)+'</td><td class="r num"><b>'+net(T.cl)+'</b></td></tr></tfoot></table></div>'
    +'<div class="cb-note">'+combos.length+' combination'+(combos.length>1?'s':'')+(d.accounts>1?' across this account and its child accounts':'')+' · balances of '+esc(((d.periods||[]).find(function(p){return String(p.lp)===String(d.lpId);})||{}).code||'')+' in '+esc(B.cur||'')+(d.truncated?' · showing the first '+combos.length:'')+'.</div>';
  $('#cdBal').html(h);
}
function select(id,scroll){S.sel=id;var a=S.byId[id];if(a)path(a).slice(0,-1).forEach(function(p){S.col[p.id]=false;});if(a)S.gcol[bucket(a.type)]=false;renderTree();renderDet();
  if(scroll!==false){var r=document.querySelector('#ctTree .ct-r[data-id="'+id+'"]');if(r&& r.scrollIntoView)r.scrollIntoView({block:'nearest'});}}
/* ---------------------------------------------------------------- form */
function modal(title,body,okLabel,onOk){$('#gpMt').text(title);$('#gpMc').html(body);$('#gpErr').removeClass('show').text('');$('#gpMok').prop('disabled',false).text(okLabel||'Save');S.onOk=onOk;
  $('#gpMb').addClass('open');setTimeout(function(){$('#gpMc').find('input:not([readonly]):not([type=checkbox]),select').first().trigger('focus');},40);}
function close(){$('#gpMb').removeClass('open');S.onOk=null;}
function v(id){return $.trim($('#'+id).val()||'');}
function ck(id){return $('#'+id).is(':checked')?'Y':'N';}
function opts(list,sel,blank){var a=(list||[]).slice();if(sel&& a.indexOf(sel)<0)a.push(sel);return (blank?'<option value="">-</option>':'')+a.map(function(x){return '<option value="'+esc(x)+'"'+(x===sel?' selected':'')+'>'+esc(lbl(x))+'</option>';}).join('');}
function rule(code){var mm=segMismatch();if(mm)return {ok:false,msg:mm};var s=accSeg();if(!s)return {ok:false,msg:'No account segment is defined for this structure.'};
  var c=U(code),mn=+s.minLen||1,mx=+s.maxLen||30,f=U(s.format),txt='Segment '+s.num+' · '+s.nameEn+': '+mn+(mx!==mn?'–'+mx:'')+' characters, '+lbl(f||'any').toLowerCase()+', upper case.';
  if(!c)return {ok:false,msg:txt,empty:true};
  if(c.length<mn||c.length>mx)return {ok:false,msg:'Must be '+mn+(mx!==mn?' to '+mx:'')+' characters ('+c.length+' now). '+txt};
  if(f==='NUMERIC'&&!/^[0-9]+$/.test(c))return {ok:false,msg:'Digits only. '+txt};
  if(f==='ALPHANUMERIC'&&!/^[A-Z0-9]+$/.test(c))return {ok:false,msg:'Letters and digits only. '+txt};
  if(!S.editing&& S.acc.some(function(a){return U(a.code)===c;}))return {ok:false,msg:'Account '+c+' already exists.'};
  return {ok:true,msg:'Valid · '+txt};}
function comb(code){var c=coa();if(!c)return '';var segs=S.segs.slice().sort(function(a,b){return a.num-b.num;});
  return segs.map(function(s){var me=+s.num===+c.accountSeg;return me?'<span class="me" title="'+esc(s.nameEn)+'">'+esc(U(code)||'\u2022\u2022\u2022\u2022')+'</span>'
    :'<span class="oth" title="'+esc(s.nameEn)+' \u2013 chosen on each journal line"><b>'+esc(s.nameEn)+'</b></span>';}).join('<i>'+esc(c.delimiter||'-')+'</i>')
    +'<div class="cf-note">Only the account code is entered here. Company, branch, cost centre and the other segments are separate fields, chosen on each journal line when you post; together they form the account combination.</div>';}
function typeDef(t){return S.types.find(function(x){return x.type===t;})||{};}
function accForm(a,parent,group){
  var e=!!a;S.editing=e;var tpl=a||parent||null;
  if(!e){var t=parent?parent.type:((S.types.find(function(x){return bucket(x.type)===(group||'ASSET');})||{}).type||'');var d=typeDef(t);
    a={type:t,normalBalance:(parent&& parent.normalBalance)||d.normalBalance,fsClass:(parent&& parent.fsClass)||d.fsClass,cashFlowClass:(parent&& parent.cashFlowClass)||d.cashFlowClass,
      parentId:parent?parent.id:'',active:'Y',summary:'N',posting:'Y',start:''};
    FLAGS.forEach(function(f){a[f[0]]=parent?parent[f[0]]:'N';});}
  var par='<option value="">Top level (no parent)</option>'+S.acc.slice().sort(function(x,y){return String(x.code).localeCompare(String(y.code),undefined,{numeric:true});})
    .filter(function(p){if(!e)return true;return p.id!==a.id&&path(p).every(function(q){return q.id!==a.id;});})
    .map(function(p){return '<option value="'+p.id+'"'+(String(p.id)===String(a.parentId)?' selected':'')+'>'+esc(p.code+' - '+p.name)+(yn(p.summary)?'':'  (posting)')+'</option>';}).join('');
  modal(e?'Edit account '+a.code:'New GL account',
    '<div class="gp-g"><div class="cf-sec">Account</div>'
    +'<div class="gp-f"><label for="aCode">Natural account code *</label><input type="text" id="aCode" maxlength="30" value="'+esc(a.code||'')+'"'+(e?' readonly':'')+' autocomplete="off">'
    +'<div class="cf-rule" id="aRule"></div><div class="cf-comb" id="aComb"></div></div>'
    +'<div class="gp-f"><label for="aPar">Parent account</label><select id="aPar">'+par+'</select><div class="cf-rule" id="aParHint"></div></div>'
    +'<div class="gp-f"><label for="aName">Name (English) *</label><input type="text" id="aName" maxlength="240" value="'+esc(a.name||'')+'"></div>'
    +'<div class="gp-f"><label for="aNameAr">Name (Arabic)</label><input type="text" id="aNameAr" class="rtl" maxlength="240" value="'+esc(a.nameAr||'')+'"></div>'
    +'<div class="gp-f full"><label for="aDesc">Description</label><input type="text" id="aDesc" maxlength="1000" value="'+esc(a.description||'')+'"></div>'
    +'<div class="cf-sec">Classification</div>'
    +'<div class="gp-f"><label for="aType">Account type *</label><select id="aType">'+opts(S.lists.types,a.type,!a.type)+'</select></div>'
    +'<div class="gp-f"><label for="aNb">Normal balance *</label><select id="aNb">'+opts(S.lists.balances,a.normalBalance,!a.normalBalance)+'</select></div>'
    +'<div class="gp-f"><label for="aFs">Financial statement class *</label><select id="aFs">'+opts(S.lists.fsClasses,a.fsClass,!a.fsClass)+'</select></div>'
    +'<div class="gp-f"><label for="aCf">Cash flow class *</label><select id="aCf">'+opts(S.lists.cfClasses,a.cashFlowClass,!a.cashFlowClass)+'</select></div>'
    +'<div class="gp-f"><label for="aStart">Effective from</label><input type="date" id="aStart" value="'+esc(a.start||'')+'"></div><div class="gp-f"><label for="aEnd">Effective to</label><input type="date" id="aEnd" value="'+esc(a.end||'')+'"></div>'
    +'<div class="cf-sec">Controls</div><div class="cf-flags">'
    +'<label class="gp-chk"><input type="checkbox" id="aSum"'+(yn(a.summary)?' checked':'')+'> Summary (parent) account</label>'
    +'<label class="gp-chk"><input type="checkbox" id="aPost"'+(yn(a.posting)?' checked':'')+'> Posting allowed</label>'
    +'<label class="gp-chk"><input type="checkbox" id="aAct"'+(yn(a.active)?' checked':'')+'> Active</label>'
    +FLAGS.map(function(f){return '<label class="gp-chk"><input type="checkbox" id="aF_'+f[0]+'"'+(yn(a[f[0]])?' checked':'')+'> '+f[1]+'</label>';}).join('')+'</div></div>',
    e?'Save account':'Create account',function(){
      var code=U(v('aCode')),r=rule(code);
      if(!r.ok){showErr(r.msg);return;}
      if(!v('aName')||!v('aType')||!v('aNb')||!v('aFs')||!v('aCf')){showErr('Name, account type, normal balance, financial statement class and cash flow class are required.');return;}
      if(v('aEnd')&&v('aStart')&&v('aEnd')<v('aStart')){showErr('Effective to cannot be before effective from.');return;}
      var o={id:e?a.id:null,coaId:S.coaId,code:code,parentId:v('aPar'),name:v('aName'),nameAr:v('aNameAr'),description:v('aDesc'),type:v('aType'),normalBalance:v('aNb'),fsClass:v('aFs'),cashFlowClass:v('aCf'),
        start:v('aStart'),end:v('aEnd'),summary:ck('aSum'),posting:ck('aPost'),active:ck('aAct')};
      FLAGS.forEach(function(f){o[f[0]]=ck('aF_'+f[0]);});
      $('#gpMok').prop('disabled',true);
      call('GL_SAVE_ACCOUNT',{x01:JSON.stringify(o)},function(d){close();apex.message.showPageSuccess(e?'Account '+code+' saved.':'Account '+code+' created and linked to the account segment.');S.sel=d.id;load(S.coaId,true);});
    });
  function upd(){var r=rule($('#aCode').val());$('#aRule').text(r.msg).attr('class','cf-rule'+(r.empty?'':(r.ok?' ok':' bad')));$('#aComb').html(comb($('#aCode').val()));}
  function par2(){var p=S.byId[v('aPar')];$('#aParHint').text(p&&!yn(p.summary)?'This parent is a posting account; parents are usually summary accounts.':'');}
  function sum(){var on=$('#aSum').is(':checked');if(on)$('#aPost').prop('checked',false);$('#aPost').prop('disabled',on).closest('.gp-chk').css('opacity',on?.55:1);}
  $('#aCode').on('input',function(){var p=this.selectionStart;this.value=this.value.toUpperCase();try{this.setSelectionRange(p,p);}catch(x){}upd();});
  $('#aType').on('change',function(){var d=typeDef(this.value);if(d.normalBalance)$('#aNb').val(d.normalBalance);if(d.fsClass)$('#aFs').val(d.fsClass);if(d.cashFlowClass)$('#aCf').val(d.cashFlowClass);});
  $('#aPar').on('change',par2);$('#aSum').on('change',sum);upd();par2();sum();
}
/* ---------------------------------------------------------------- events */
function vis(){return [].slice.call(document.querySelectorAll('#ctTree .ct-r')).map(function(r){return r.getAttribute('data-id');});}
$(function(){
  $('#ctCoa').on('change',function(){load(this.value,false);});
  $('#ctAdd').on('click',function(){var p=S.sel&& S.byId[S.sel];accForm(null,p&&yn(p.summary)?p:null,p?bucket(p.type):'ASSET');});
  var t;$('#ctQ').on('input',function(){var q=this.value;clearTimeout(t);t=setTimeout(function(){S.q=U(q);renderTree();},120);});
  $('#ctChips').on('click','[data-f]',function(){S.flt=$(this).data('f');render();});
  $('#ctExp').on('click',function(){S.col={};S.gcol={};renderTree();});
  $('#ctCol').on('click',function(){S.acc.forEach(function(a){if(kids(a.id).length)S.col[a.id]=true;});renderTree();});
  $('#ctTree').on('click','.ct-grp',function(e){if($(e.target).closest('[data-ag]').length)return;var g=$(this).data('g');S.gcol[g]=!S.gcol[g];renderTree();})
    .on('click','[data-ag]',function(e){e.stopPropagation();accForm(null,null,$(this).data('ag'));})
    .on('click','[data-t]',function(e){e.stopPropagation();var id=$(this).data('t');S.col[id]=!S.col[id];renderTree();})
    .on('click','.ct-r',function(){select($(this).data('id'),false);document.getElementById('ctTree').focus({preventScroll:true});})
    .on('dblclick','.ct-r',function(){var a=S.byId[$(this).data('id')];if(a)accForm(a);})
    .on('keydown',function(e){var ids=vis(),i=ids.indexOf(String(S.sel)),a=S.sel&& S.byId[S.sel];
      if(e.key==='ArrowDown'){e.preventDefault();select(ids[Math.min(ids.length-1,i+1)]||ids[0]);}
      else if(e.key==='ArrowUp'){e.preventDefault();select(ids[Math.max(0,i-1)]||ids[0]);}
      else if(e.key==='ArrowRight'&&a&&kids(a.id).length){e.preventDefault();S.col[a.id]=false;renderTree();}
      else if(e.key==='ArrowLeft'&&a){e.preventDefault();if(kids(a.id).length&&!S.col[a.id]){S.col[a.id]=true;renderTree();}else if(a.parentId&& S.byId[a.parentId])select(a.parentId);}
      else if(e.key==='Enter'&&a){e.preventDefault();accForm(a);}});
  $('#ctDet').on('change','#cbLed',function(){S.bal.cur=null;loadBal(S.sel,this.value,'');})
    .on('change','#cbLp',function(){loadBal(S.sel,S.bal.led,this.value);})
    .on('change','#cbCur',function(){S.bal.cur=this.value;renderBal();})
    .on('change','#cbNz',function(){S.bal.nz=this.checked;renderBal();})
    .on('click','[data-gb]',function(){S.bal.group=String($(this).data('gb'));renderBal();});
  $('#ctDet').on('click','[data-go]',function(){select($(this).data('go'));})
    .on('click','#cdEdit',function(){var a=S.byId[S.sel];if(a)accForm(a);})
    .on('click','#cdChild',function(){var a=S.byId[S.sel];if(a)accForm(null,a,bucket(a.type));});
  $('#gpMx,#gpMcancel').on('click',close);
  $('#gpMb').on('click',function(e){if(e.target===this)close();});
  $(document).on('keydown',function(e){if(e.key==='Escape'&&$('#gpMb').hasClass('open'))close();});
  $('#gpMok').on('click',function(){if(S.onOk)S.onOk();});
  load(null,false);
});
})(apex.jQuery);