(function($){
'use strict';
var S={coaId:null,coas:[],segs:[],ledgers:[],combos:0,types:[],formats:[],les:[],bls:[],seg:null,vals:[],onOk:null};
var ICO_EDIT='<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 20h9"/><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z"/></svg>',ICO_PLUS='<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 5v14M5 12h14"/></svg>',ICO_STAR='<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m12 3 2.7 5.6 6.1.9-4.4 4.3 1 6.1L12 17l-5.4 2.9 1-6.1-4.4-4.3 6.1-.9z"/></svg>';
var ROLES=[['account','Account'],['balancing','Balancing'],['branch','Branch'],['costCenter','Cost centre'],['businessLine','Business line'],['intercompany','Intercompany']];
function esc(v){return apex.util.escapeHTML(v==null?'':String(v));}
function lbl(v){v=String(v||'').replace(/_/g,' ').toLowerCase();return v.replace(/\b\w/g,function(c){return c.toUpperCase();});}
function yn(v){return String(v||'').trim()==='Y';}
function fd(d){if(!d)return '-';var p=d.split('-');var m=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];return p[2]+' '+m[+p[1]-1]+' '+p[0];}
function today(){var t=new Date();function z(n){return (n<10?'0':'')+n;}return t.getFullYear()+'-'+z(t.getMonth()+1)+'-'+z(t.getDate());}
function coa(){return S.coas.find(function(c){return String(c.id)===String(S.coaId);});}
function seg(){return S.segs.find(function(s){return String(s.id)===String(S.seg);});}
function call(name,data,ok){
  apex.server.process(name,data,{dataType:'json'}).then(function(d){if(d&& d.ok===false){showErr(d.message||'The request failed.');return;}ok(d||{});},
    function(x){showErr((x&& x.responseText)||'The request failed.');});
}
function showErr(m){m=String(m||'').replace(/^ORA-\d+:\s*/,'').replace(/\nORA-[\s\S]*$/,'');
  if($('#gpMb').hasClass('open')){$('#gpErr').text(m).addClass('show');$('#gpMok').prop('disabled',false);}else{apex.message.clearErrors();apex.message.showErrors([{type:'error',location:'page',message:m,unsafe:false}]);}}
function roles(n){var c=coa();if(!c)return [];return ROLES.filter(function(r){return c[r[0]]!=null&&String(c[r[0]])===String(n);}).map(function(r){return r[1];});}
function load(coaId){
  call('SEG_LOAD',{x01:coaId||''},function(d){
    S.coaId=d.coaId;S.coas=d.structures||[];S.segs=d.segments||[];S.ledgers=d.ledgers||[];S.combos=d.combinations||0;
    S.types=d.types||[];S.formats=d.formats||[];S.les=d.legalEntities||[];S.bls=d.businessLines||[];
    if(!S.segs.some(function(s){return String(s.id)===String(S.seg);}))S.seg=S.segs[0]?S.segs[0].id:null;
    render();loadVals();
  });
}
function loadVals(){if(!S.seg){S.vals=[];renderVals();return;}call('SEG_VALUES',{x01:S.seg},function(d){S.vals=d.values||[];renderVals();});}
function render(){
  $('#sgCoa').html(S.coas.length?S.coas.map(function(c){return '<option value="'+c.id+'"'+(String(c.id)===String(S.coaId)?' selected':'')+'>'+esc(c.code+' - '+c.nameEn)+'</option>';}).join(''):'<option value="">No structure yet</option>');
  $('#sgEdit').prop('disabled',!S.coaId);
  var c=coa(),h='';
  if(c){var fmt=S.segs.map(function(s){var n=Math.max(1,Math.min(+s.maxLen||+s.minLen||3,10));return '<span title="'+esc(s.nameEn)+'">'+esc(s.code)+'</span>';}).join('<i>'+esc(c.delimiter||'-')+'</i>');
    h='<div class="gp-cal"><div><span class="k">Structure</span><span class="v">'+esc(c.nameEn)+(c.nameAr?' <span style="font-weight:500;color:#5F7B8A">· '+esc(c.nameAr)+'</span>':'')+'</span></div>'
     +'<div><span class="k">Segments</span><span class="v">'+S.segs.length+' of '+esc(c.segCount)+'</span></div>'
     +'<div><span class="k">Delimiter</span><span class="v">'+esc(c.delimiter||'-')+'</span></div>'
     +'<div><span class="k">Dynamic combinations</span><span class="v">'+(yn(c.dynamic)?'Allowed':'No')+'</span></div>'
     +'<div><span class="k">Combinations</span><span class="v">'+esc(S.combos)+'</span></div>'
     +'<div><span class="k">Status</span><span class="v">'+(yn(c.active)?'<span class="gp-pill ok">Active</span>':'<span class="gp-pill off">Inactive</span>')+'</span></div>'
     +'<div class="led"><span class="k" style="margin-right:4px">Ledgers</span>'+(S.ledgers.length?S.ledgers.map(function(l){return '<span class="gp-tag">'+esc(l)+'</span>';}).join(''):'<span class="gp-tag">None</span>')+'</div>'
     +(S.segs.length?'<div style="flex-basis:100%"><span class="k">Account format</span><div class="sg-fmt">'+fmt+'</div></div>':'')+'</div>';}
  $('#sgInfo').html(h);
  if(!c){$('#sgSegs').html('<div class="gp-empty" style="grid-column:1/-1;background:#fff;border:1px dashed #DFE8EC;border-radius:14px"><b>No account structure yet</b>Create one with <b style="display:inline">New structure</b>.</div>');return;}
  $('#sgSegs').html(S.segs.map(function(s){var r=roles(s.num);
    return '<button type="button" class="sg-seg'+(String(s.id)===String(S.seg)?' on':'')+'" data-seg="'+s.id+'"><span class="r1"><span class="sg-no">'+esc(s.num)+'</span>'
      +(yn(s.enabled)?'<span class="gp-pill ok">Enabled</span>':'<span class="gp-pill off">Disabled</span>')+'</span>'
      +'<b>'+esc(s.nameEn)+'</b><small><code style="font:600 11.5px ui-monospace,Menlo,Consolas,monospace;color:#00636D">'+esc(s.code)+'</code> · '+esc(lbl(s.type))+'</small>'
      +'<small>Length '+esc(s.minLen)+(s.maxLen!=s.minLen?'–'+esc(s.maxLen):'')+' · '+esc(s.values)+' values'+(yn(s.required)?' · required':'')+'</small>'
      +(s.defValue?'<span class="sg-def">Journal default <code>'+esc(s.defValue)+'</code>'+(s.defMode==='FIXED'?' &middot; always':'')+'</span>':'')
      +(r.length?'<span class="sg-roles">'+r.map(function(x){return '<span class="sg-role">'+esc(x)+'</span>';}).join('')+'</span>':'')+'</button>';}).join('')
    +(S.segs.length<(+c.segCount||99)?'<button type="button" class="sg-seg add" id="sgAddSeg">'+ICO_PLUS+'<span>Add segment</span></button>':''));
}
function depth(v,map){var d=0,cur=v,g=0;while(cur&& cur.parentId&&map[cur.parentId]&&g++<20){d++;cur=map[cur.parentId];}return d;}
function ordered(){var map={},kids={},roots=[],out=[];S.vals.forEach(function(v){map[v.id]=v;});
  S.vals.forEach(function(v){if(v.parentId&&map[v.parentId]){(kids[v.parentId]=kids[v.parentId]||[]).push(v);}else roots.push(v);});
  function walk(v,d){v._d=d;out.push(v);(kids[v.id]||[]).forEach(function(k){walk(k,d+1);});}
  roots.forEach(function(r){walk(r,0);});return out;}
function renderVals(){
  var s=seg();if(!s){$('#sgVals').html('<div class="gp-empty">'+(S.coaId?'Add a segment to start defining values.':'')+'</div>');return;}
  var q=String($('#sgQ').val()||'').toUpperCase(),rows=ordered().filter(function(v){return !q||(v.code+' '+v.nameEn+' '+(v.nameAr||'')).toUpperCase().indexOf(q)>=0;});
  var h='<div class="gp-ph"><h3>'+esc(s.nameEn)+'<small>'+S.vals.length+' values · segment '+esc(s.num)+'</small></h3><div class="sg-bar">'
    +'<input type="search" class="sg-q" id="sgQ" placeholder="Search values" value="'+esc($('#sgQ').val()||'')+'">'
    +(s.defReady&& s.type!=='ACCOUNT'?'<button type="button" class="gp-btn gp-btn--sm" id="sgDef" title="Value filled in automatically on every journal line">'+ICO_STAR+'<span>Journal entry default</span></button>':'')
    +'<button type="button" class="gp-btn gp-btn--sm" id="sgSegEdit">'+ICO_EDIT+'<span>Edit segment</span></button>'
    +'<button type="button" class="gp-btn gp-btn--sm gp-btn--pri" id="sgAddVal">'+ICO_PLUS+'<span>Add value</span></button></div></div>';
  if(!rows.length)h+='<div class="gp-empty">'+(S.vals.length?'No value matches the search.':'<b>No values yet</b>Add the first value of this segment.')+'</div>';
  else{h+='<div class="gp-tw"><table class="gp-t"><thead><tr><th>Code</th><th>Name</th><th>Parent</th><th>Summary</th><th>Posting</th><th>Effective</th><th>Status</th><th></th></tr></thead><tbody>';
    rows.forEach(function(v){h+='<tr><td><span class="sg-ind" style="width:'+(q?0:v._d*16)+'px"></span><code>'+esc(v.code)+'</code>'+(s.defValue===v.code?' <span class="gp-pill ok" title="Journal entry default">Default</span>':'')+'</td><td class="sg-name">'+esc(v.nameEn)+(v.nameAr?'<span class="ar">'+esc(v.nameAr)+'</span>':'')+'</td>'
      +'<td>'+(v.parentCode?'<code>'+esc(v.parentCode)+'</code>':'-')+'</td><td class="'+(yn(v.summary)?'yes':'no')+'">'+(yn(v.summary)?'Yes':'No')+'</td><td class="'+(yn(v.posting)?'yes':'no')+'">'+(yn(v.posting)?'Yes':'No')+'</td>'
      +'<td class="num">'+fd(v.start)+(v.end?' &ndash; '+fd(v.end):'')+'</td><td>'+(yn(v.enabled)?'<span class="gp-pill ok">Enabled</span>':'<span class="gp-pill off">Disabled</span>')+'</td>'
      +'<td style="white-space:nowrap">'+(s.defReady&& s.type!=='ACCOUNT'&&yn(v.posting)&&yn(v.enabled)&&!yn(v.summary)
          ?'<button type="button" class="gp-btn gp-btn--sm'+(s.defValue===v.code?' sg-star-on':'')+'" data-defval="'+esc(v.code)+'" title="'+(s.defValue===v.code?'Journal entry default (click to change)':'Make this the journal entry default')+'" aria-label="Journal entry default '+esc(v.code)+'" style="margin-right:6px">'+ICO_STAR+'</button>':'')
      +'<button type="button" class="gp-btn gp-btn--sm" data-val="'+v.id+'" aria-label="Edit '+esc(v.code)+'">'+ICO_EDIT+'</button></td></tr>';});
    h+='</tbody></table></div>';}
  var foc=document.activeElement&& document.activeElement.id==='sgQ',pos=foc?document.activeElement.selectionStart:0;
  $('#sgVals').html(h);if(foc){var el=document.getElementById('sgQ');el.focus();el.setSelectionRange(pos,pos);}
}
/* ---------- modal */
function modal(title,body,okLabel,onOk){$('#gpMt').text(title);$('#gpMc').html(body);$('#gpErr').removeClass('show').text('');$('#gpMok').prop('disabled',false).text(okLabel||'Save');S.onOk=onOk;
  $('#gpMb').addClass('open');setTimeout(function(){$('#gpMc').find('input:not([readonly]):not([type=checkbox]),select').first().trigger('focus');},40);}
function close(){$('#gpMb').removeClass('open');S.onOk=null;}
function v(id){return $.trim($('#'+id).val()||'');}
function ck(id){return $('#'+id).is(':checked')?'Y':'N';}
function fld(id,label,val,opt){opt=opt||{};return '<div class="gp-f'+(opt.full?' full':'')+'"><label for="'+id+'">'+label+'</label><input type="'+(opt.type||'text')+'" id="'+id+'"'+(opt.max?' maxlength="'+opt.max+'"':'')
  +(opt.list?' list="'+opt.list+'"':'')+(opt.ro?' readonly':'')+(opt.cls?' class="'+opt.cls+'"':'')+(opt.min!=null?' min="'+opt.min+'"':'')+' value="'+esc(val==null?'':val)+'">'+(opt.hint?'<div class="gp-hint">'+opt.hint+'</div>':'')+'</div>';}
function chk(id,label,on){return '<label class="gp-chk"><input type="checkbox" id="'+id+'"'+(on?' checked':'')+'> '+label+'</label>';}
function segOpts(sel){return '<option value="">-</option>'+S.segs.map(function(s){return '<option value="'+s.num+'"'+(String(s.num)===String(sel)?' selected':'')+'>'+esc(s.num+' · '+s.nameEn)+'</option>';}).join('');}
function numOpts(n,sel,req){var o=req?'':'<option value="">-</option>';
  for(var i=1;i<=n;i++){var s=S.segs.find(function(x){return +x.num===i;});o+='<option value="'+i+'"'+(String(sel)===String(i)?' selected':'')+'>'+i+(s?' \u00b7 '+esc(s.nameEn):'')+'</option>';}return o;}
function selNum(id,label,sel,req,n){return '<div class="gp-f"><label for="'+id+'">'+label+'</label><select id="'+id+'" data-req="'+(req?1:0)+'">'+numOpts(n,sel,req)+'</select></div>';}
var ROLE_IDS=['fAcc','fBal','fBr','fCc','fBl','fIc'];
function structForm(c){
  var e=!!c;c=c||{delimiter:'-',segCount:5,dynamic:'N',approval:'N',active:'Y',start:today()};var n=Math.min(10,Math.max(1,+c.segCount||1));
  modal(e?'Edit account structure':'New account structure',
    '<div class="gp-g">'+fld('fCode','Structure code *',c.code,{max:30,ro:e,hint:e?'The code identifies the structure and cannot be changed.':''})+fld('fNameEn','Name (English) *',c.nameEn,{max:240})
    +fld('fNameAr','Name (Arabic)',c.nameAr,{max:240,cls:'rtl'})+fld('fDesc','Description',c.description,{max:1000})
    +fld('fDelim','Delimiter',c.delimiter,{max:3})+fld('fCount','Number of segments *',c.segCount,{type:'number',min:1,hint:'1 to 10. The segment lists below follow this number.'})
    +selNum('fAcc','Account segment *',c.account,true,n)+selNum('fBal','Balancing segment',c.balancing,false,n)
    +selNum('fBr','Branch segment',c.branch,false,n)+selNum('fCc','Cost centre segment',c.costCenter,false,n)
    +selNum('fBl','Business line segment',c.businessLine,false,n)+selNum('fIc','Intercompany segment',c.intercompany,false,n)
    +fld('fStart','Effective from',c.start,{type:'date'})+fld('fEnd','Effective to',c.end,{type:'date'})
    +chk('fDyn','Allow dynamic combinations',yn(c.dynamic))+chk('fAppr','Combinations need approval',yn(c.approval))+chk('fAct','Active',yn(c.active))+'</div>',
    'Save structure',function(){
      if(!v('fCode')||!v('fNameEn')||!v('fCount')||!v('fAcc')){showErr('Code, English name, number of segments and the account segment are required.');return;}
      var cnt=+v('fCount');if(!(cnt>=1&&cnt<=10)||cnt!==Math.floor(cnt)){showErr('The number of segments must be between 1 and 10.');return;}
      var badR=ROLE_IDS.filter(function(id){return v(id)&&(+v(id)<1||+v(id)>cnt);});
      if(badR.length){showErr('Each segment role must be a segment number between 1 and '+cnt+'.');return;}
      if(v('fEnd')&&v('fStart')&&v('fEnd')<v('fStart')){showErr('Effective to cannot be before effective from.');return;}
      $('#fCode').val(v('fCode').toUpperCase());
      $('#gpMok').prop('disabled',true);
      call('SEG_SAVE_STRUCT',{x01:JSON.stringify({code:v('fCode'),nameEn:v('fNameEn'),nameAr:v('fNameAr'),description:v('fDesc'),delimiter:v('fDelim'),segCount:v('fCount'),
        account:v('fAcc'),balancing:v('fBal'),branch:v('fBr'),costCenter:v('fCc'),businessLine:v('fBl'),intercompany:v('fIc'),
        dynamic:ck('fDyn'),approval:ck('fAppr'),active:ck('fAct'),start:v('fStart'),end:v('fEnd')})},
        function(d){close();apex.message.showPageSuccess('Account structure saved.');load(d.id);});
    });
  $('#fCount').on('input change',function(){var k=Math.min(10,Math.max(1,parseInt(this.value,10)||1));
    ROLE_IDS.forEach(function(id){var $s=$('#'+id),cur=$s.val();$s.html(numOpts(k,cur,$s.data('req')===1));});});
}
var SEG_TYPES=['COMPANY','BRANCH','COST_CENTER','ACCOUNT','BUSINESS_LINE','INTERCOMPANY','PROJECT','CUSTOMER','CONTRACT','FUTURE','CUSTOM'];
var VALUE_FORMATS=['ALPHANUMERIC','NUMERIC','CHARACTER'];
function opts(list,sel){var a=list.slice();if(sel&& a.indexOf(sel)<0)a.push(sel);return a.map(function(x){return '<option value="'+esc(x)+'"'+(x===sel?' selected':'')+'>'+esc(lbl(x))+'</option>';}).join('');}
function dl(id,list){return '<datalist id="'+id+'">'+list.map(function(x){return '<option value="'+esc(x)+'">';}).join('')+'</datalist>';}
function segForm(s){
  var c=coa();if(!c)return;var e=!!s;
  if(!e){var used=S.segs.map(function(x){return +x.num;}),n=1;while(used.indexOf(n)>=0)n++;s={num:n,minLen:1,maxLen:10,upper:'Y',required:'Y',summary:'N',enabled:'Y',seq:n*10};}
  modal(e?'Edit segment '+s.code:'Add segment to '+c.code,
    '<div class="gp-g">'+fld('sNum','Segment number *',s.num,{type:'number',min:1,ro:e})+fld('sCode','Segment code *',s.code,{max:30,ro:e,hint:e?'Number and code identify the segment and cannot be changed.':''})
    +fld('sNameEn','Name (English) *',s.nameEn,{max:150})+fld('sNameAr','Name (Arabic)',s.nameAr,{max:150,cls:'rtl'})
    +'<div class="gp-f"><label for="sType">Segment type *</label><select id="sType">'+(s.type?'':'<option value="">-</option>')+opts(SEG_TYPES,s.type)+'</select></div>'
    +'<div class="gp-f"><label for="sFmt">Value format *</label><select id="sFmt">'+opts(VALUE_FORMATS,s.format||'ALPHANUMERIC')+'</select></div>'
    +'</div><div class="gp-g3" style="margin-top:14px">'+fld('sMin','Min length *',s.minLen,{type:'number',min:1})+fld('sMax','Max length *',s.maxLen,{type:'number',min:1,hint:'Up to 30.'})+fld('sPad','Pad character',s.pad,{max:1})+'</div>'
    +'<div class="gp-g" style="margin-top:14px">'+fld('sSeq','Display sequence',s.seq,{type:'number',min:0})+fld('sDesc','Description',s.description,{max:1000})
    +chk('sUp','Upper-case values',yn(s.upper))+chk('sReq','Required',yn(s.required))+chk('sSum','Allow summary values',yn(s.summary))+chk('sEn','Enabled',yn(s.enabled))+'</div>'
    ,
    'Save segment',function(){
      if(!v('sNum')||!v('sCode')||!v('sNameEn')||!v('sType')){showErr('Number, code, English name and type are required.');return;}
      var mn=+v('sMin'),mxl=+v('sMax');if(!(mn>=1&&mxl>=mn&&mxl<=30)){showErr('Lengths must satisfy 1 \u2264 min length \u2264 max length \u2264 30.');return;}
      $('#sCode').val(v('sCode').toUpperCase());
      $('#gpMok').prop('disabled',true);
      call('SEG_SAVE_SEGMENT',{x01:JSON.stringify({coa:c.code,num:v('sNum'),code:v('sCode'),nameEn:v('sNameEn'),nameAr:v('sNameAr'),type:v('sType'),minLen:v('sMin'),maxLen:v('sMax'),
        format:v('sFmt'),pad:$('#sPad').val(),upper:ck('sUp'),required:ck('sReq'),summary:ck('sSum'),enabled:ck('sEn'),seq:v('sSeq'),description:v('sDesc')})},
        function(d){close();apex.message.showPageSuccess('Segment saved.');S.seg=d.id;load(S.coaId);});
    });
}
function defForm(s,pick){
  var list=S.vals.filter(function(x){return yn(x.posting)&&yn(x.enabled)&&!yn(x.summary);});
  var cur=pick||s.defValue||'',mode=s.defMode||'SUGGEST';
  modal('Journal entry default - '+s.nameEn,
    '<div class="gp-ready">The default is filled in on every new journal line, so users do not have to type it. Useful when the company has one '+esc(s.nameEn.toLowerCase())+', or one is used most of the time.</div>'
    +'<div class="gp-f"><label for="dVal">Default value</label><select id="dVal"><option value="">No default</option>'
    +list.map(function(x){return '<option value="'+esc(x.code)+'"'+(x.code===cur?' selected':'')+'>'+esc(x.code+' - '+x.nameEn)+'</option>';}).join('')+'</select>'
    +'<div class="gp-hint">Only enabled values that allow posting are listed.</div></div>'
    +'<div class="gp-f" style="margin-top:14px"><label>How it is used</label>'
    +'<label class="gp-chk" style="margin-bottom:8px"><input type="radio" name="dMode" value="SUGGEST"'+(mode!=='FIXED'?' checked':'')+'> Suggest it - filled in, the user can change it on a line</label>'
    +'<label class="gp-chk"><input type="radio" name="dMode" value="FIXED"'+(mode==='FIXED'?' checked':'')+'> Always use it - the field is hidden in journal entry (one '+esc(s.nameEn.toLowerCase())+' only)</label></div>',
    'Save default',function(){var val=v('dVal'),md=$('input[name=dMode]:checked').val()||'SUGGEST';
      $('#gpMok').prop('disabled',true);
      call('SEG_SAVE_DEFAULT',{x01:s.id,x02:val,x03:md},function(){close();apex.message.showPageSuccess(val?'Default saved: new journal lines get '+val+'.':'Default removed.');load(S.coaId);});});
}
function valForm(x){
  var c=coa(),s=seg();if(!c||!s)return;var e=!!x;
  if(!e){x={summary:'N',posting:'Y',enabled:'Y',start:today(),sort:(S.vals.length+1)*10};}
  var par='<option value="">No parent</option>'+ordered().filter(function(p){return !e||p.id!==x.id;}).map(function(p){return '<option value="'+esc(p.code)+'"'+(p.code===x.parentCode?' selected':'')+'>'+'  '.repeat(p._d||0)+esc(p.code+' - '+p.nameEn)+'</option>';}).join('');
  var le='<option value="">-</option>'+S.les.map(function(l){return '<option value="'+esc(l.code)+'"'+(l.code===x.leCode?' selected':'')+'>'+esc(l.code+' - '+l.name)+'</option>';}).join('');
  var bl='<option value="">-</option>'+S.bls.map(function(l){return '<option value="'+esc(l.code)+'"'+(l.code===x.blCode?' selected':'')+'>'+esc(l.code+' - '+l.name)+'</option>';}).join('');
  var mx=+s.maxLen||null;
  modal(e?'Edit value '+x.code:'Add value to '+s.nameEn,
    '<div class="gp-g">'+fld('vCode','Value code *',x.code,{max:mx||30,ro:e,hint:e?'The code identifies the value and cannot be changed.':'Length '+esc(s.minLen)+(s.maxLen!=s.minLen?'\u2013'+esc(s.maxLen):'')+' characters, upper case.'})
    +fld('vSort','Sort order',x.sort,{type:'number',min:0})
    +fld('vNameEn','Name (English) *',x.nameEn,{max:240})+fld('vNameAr','Name (Arabic)',x.nameAr,{max:240,cls:'rtl'})
    +'<div class="gp-f full"><label for="vPar">Parent value</label><select id="vPar">'+par+'</select></div>'
    +fld('vStart','Effective from *',x.start,{type:'date'})+fld('vEnd','Effective to',x.end,{type:'date'})
    +'<div class="gp-f"><label for="vLe">Legal entity</label><select id="vLe">'+le+'</select></div>'
    +'<div class="gp-f"><label for="vBl">Business line</label><select id="vBl">'+bl+'</select></div>'
    +fld('vDesc','Description',x.description,{max:1000,full:true})
    +chk('vSum','Summary (parent) value',yn(x.summary))+chk('vPost','Posting allowed',yn(x.posting))+chk('vEn','Enabled',yn(x.enabled))+'</div>',
    'Save value',function(){
      if(!v('vCode')||!v('vNameEn')||!v('vStart')){showErr('Code, English name and effective date are required.');return;}
      var code=v('vCode').toUpperCase();
      if($('#vSum').is(':checked')&&$('#vPost').is(':checked')){showErr('A summary value cannot allow posting.');return;}
      if(v('vEnd')&&v('vEnd')<v('vStart')){showErr('Effective to cannot be before effective from.');return;}
      if(s.minLen&& code.length<+s.minLen||s.maxLen&& code.length>+s.maxLen){showErr('The code must be '+s.minLen+(s.maxLen!=s.minLen?' to '+s.maxLen:'')+' characters long.');return;}
      $('#gpMok').prop('disabled',true);
      call('SEG_SAVE_VALUE',{x01:JSON.stringify({id:e?x.id:null,coa:c.code,segment:s.code,code:code,nameEn:v('vNameEn'),nameAr:v('vNameAr'),description:v('vDesc'),parent:v('vPar'),
        summary:ck('vSum'),posting:ck('vPost'),enabled:ck('vEn'),start:v('vStart'),end:v('vEnd'),sort:v('vSort'),le:v('vLe'),bl:v('vBl')})},
        function(){close();apex.message.showPageSuccess('Value saved.');load(S.coaId);});
    });
  function sumPost(){var on=$('#vSum').is(':checked');if(on)$('#vPost').prop('checked',false);$('#vPost').prop('disabled',on).closest('.gp-chk').css('opacity',on?.55:1);}
  $('#vSum').on('change',sumPost);sumPost();
}
$(function(){
  $('#sgCoa').on('change',function(){S.seg=null;load(this.value);});
  $('#sgNew').on('click',function(){structForm(null);});
  $('#sgEdit').on('click',function(){var c=coa();if(c)structForm(c);});
  $('#sgSegs').on('click','[data-seg]',function(){S.seg=$(this).data('seg');render();loadVals();}).on('click','#sgAddSeg',function(){segForm(null);});
  $('#sgVals').on('input','#sgQ',renderVals).on('click','#sgAddVal',function(){valForm(null);}).on('click','#sgSegEdit',function(){segForm(seg());}).on('click','#sgDef',function(){defForm(seg());}).on('click','[data-defval]',function(){defForm(seg(),String($(this).data('defval')));})
    .on('click','[data-val]',function(){var id=$(this).data('val');valForm(S.vals.find(function(x){return String(x.id)===String(id);}));});
  $('#gpMx,#gpMcancel').on('click',close);
  $('#gpMb').on('click',function(e){if(e.target===this)close();});
  $(document).on('keydown',function(e){if(e.key==='Escape'&&$('#gpMb').hasClass('open'))close();});
  $('#gpMok').on('click',function(){if(S.onOk)S.onOk();});
  load(null);
});
})(apex.jQuery);