
window.FXW=window.FXW||{};
(function(W){
  W.ar=function(){return document.documentElement.dir==='rtl';};
  W.fmt=function(n){n=+n||0;var a=Math.abs(n);return a>=1e9?(n/1e9).toFixed(2)+'B':a>=1e6?(n/1e6).toFixed(1)+'M':a>=1e3?(n/1e3).toFixed(0)+'K':String(Math.round(n));};
  W.esc=function(t){var d=document.createElement('div');d.textContent=t==null?'':String(t);return d.innerHTML;};
  W.toast=function(msg,err){var box=document.getElementById('wbToasts');if(!box)return;var t=document.createElement('div');t.className='wb-toast'+(err?' err':'');
    t.innerHTML='<span class="fa '+(err?'fa-exclamation-circle':'fa-check-circle')+'" aria-hidden="true"></span><span>'+W.esc(msg)+'</span>';box.appendChild(t);
    setTimeout(function(){t.style.transition='opacity .3s';t.style.opacity='0';setTimeout(function(){t.remove();},300);},err?5200:2800);};
  W.later=function(msg){try{sessionStorage.setItem('fxwToast',msg);}catch(e){}};
  W.flush=function(){try{var m=sessionStorage.getItem('fxwToast');if(m){sessionStorage.removeItem('fxwToast');W.toast(m);}}catch(e){}};
  var last=null;
  W.modal=function(o){
    var mask=document.getElementById('wbMask'),m=mask.querySelector('.wb-modal');last=document.activeElement;
    m.className='wb-modal '+(o.tone||'');
    m.querySelector('.wb-m-ico').innerHTML='<span class="fa '+(o.icon||'fa-check-circle')+'" aria-hidden="true"></span>';
    m.querySelector('h2').textContent=o.title;m.querySelector('p').innerHTML=o.body||'';
    m.querySelector('.wb-m-sum').innerHTML=o.summary||'';
    var yes=m.querySelector('[data-m=yes]'),no=m.querySelector('[data-m=no]');
    yes.className='fxd-btn '+(o.okClass||'fxd-btn--teal');yes.textContent=o.ok||'OK';yes.disabled=false;
    no.hidden=!o.cancel;no.textContent=o.cancel||'';
    mask._onYes=o.onOk||null;mask.classList.add('on');setTimeout(function(){yes.focus();},60);
  };
  W.closeModal=function(){var mask=document.getElementById('wbMask');mask.classList.remove('on');if(last&&last.focus)last.focus();};
  W.busy=function(on){var y=document.querySelector('#wbMask [data-m=yes]');if(y){y.disabled=on;}};
  W.openDrawer=function(){last=document.activeElement;document.getElementById('wbScrim').classList.add('on');var d=document.getElementById('wbDrawer');d.classList.add('on');d.setAttribute('aria-hidden','false');setTimeout(function(){var f=d.querySelector('button,input,select,textarea');if(f)f.focus();},260);};
  W.closeDrawer=function(){document.getElementById('wbScrim').classList.remove('on');var d=document.getElementById('wbDrawer');d.classList.remove('on');d.setAttribute('aria-hidden','true');if(last&&last.focus)last.focus();};
  W.chip=function(group,v){[].forEach.call(document.querySelectorAll(group+' button'),function(b){b.setAttribute('aria-pressed',b.getAttribute('data-v')===v?'true':'false');});};
  W.chipVal=function(group){var b=document.querySelector(group+' button[aria-pressed=true]');return b?b.getAttribute('data-v'):'';};
  document.addEventListener('click',function(e){
    var mask=document.getElementById('wbMask');
    if(mask&&mask.classList.contains('on')){var b=e.target.closest('[data-m]');
      if(b&&b.getAttribute('data-m')==='yes'){if(mask._onYes){mask._onYes();}else{W.closeModal();}return;}
      if(b||e.target===mask){W.closeModal();return;}}
    if(e.target.closest('[data-close-drawer]')||e.target.id==='wbScrim'){W.closeDrawer();return;}
    var c=e.target.closest('.wb-chips button');if(c){var g=c.parentNode;[].forEach.call(g.querySelectorAll('button'),function(x){x.setAttribute('aria-pressed',x===c?'true':'false');});}
  });
  document.addEventListener('keydown',function(e){if(e.key!=='Escape')return;var mask=document.getElementById('wbMask');
    if(mask&&mask.classList.contains('on')){W.closeModal();return;}var d=document.getElementById('wbDrawer');if(d&&d.classList.contains('on'))W.closeDrawer();});
  W.call=function(proc,data,items){return new Promise(function(res,rej){
    apex.server.process(proc,data,{pageItems:items,dataType:'json',success:function(d){if(d&&d.success===false){rej(d.message||'The request was not completed.');}else{res(d||{});}},
      error:function(j,s,e){rej((j&&j.responseText)||e||s||'Server error');}});});};
})(window.FXW);

(function(){
  var W=window.FXW;
  function init(){
    var root=document.getElementById('dsRoot'); if(!root)return;
    var ITEMS='#P217_PORTFOLIO_ID,#P217_TASK_ID',A=function(k){return root.getAttribute('data-'+k);};
    var cur=A('cur')||'',banks=[];try{banks=JSON.parse(A('banks')||'[]');}catch(e){banks=[];}
    var st={tab:'sch',acc:'ALL',d:null,timer:null,seq:0,first:true,mode:{M:'PRORATE',A:'PRORATE',L:'IMMEDIATE',V:'IMMEDIATE'}};
    function L(en,ar){return W.ar()?ar:en;}
    function q(s){return root.querySelector(s);}
    function qa(s){return [].slice.call(root.querySelectorAll(s));}
    function m2(n){return (+n||0).toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2});}
    function e(t){return W.esc(t);}
    function dt(s){if(!s)return '-';var p=s.split('-');var mo=['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];return p[2]+' '+mo[+p[1]-1]+' '+p[0];}
    function row(k){return q('.ds-ch tr[data-ch="'+k+'"]');}
    function done(){return st.d&& st.d.mode==='DONE';}
    // ---------- bank -> account cascade
    function fillBanks(){var s=q('#dsBank'),seen={},h='<option value="">'+e(L('Choose a bank','اختر البنك'))+'</option>';
      banks.forEach(function(b){if(!seen[b.b]){seen[b.b]=1;h+='<option value="'+b.b+'">'+e(b.bn)+'</option>';}});s.innerHTML=h;}
    function fillAccs(keep){var b=q('#dsBank').value,s=q('#dsAcc'),list=banks.filter(function(x){return String(x.b)===b;}),old=keep?s.value:'';
      var h='<option value="">'+e(b?L('Choose an account','اختر الحساب'):L('Choose the bank first','اختر البنك أولاً'))+'</option>';
      list.forEach(function(x){h+='<option value="'+x.a+'">'+e(x.an+(x.t?' · '+x.t:'')+(x.p&& x.p!=='---'?' · '+x.p:''))+'</option>';});
      s.innerHTML=h;if(old&& list.some(function(x){return String(x.a)===old;}))s.value=old;else if(list.length===1)s.value=String(list[0].a);}
    // ---------- charges: '' = financial review amount, waived = 0
    function chargeVal(k){var tr=row(k);if(tr.querySelector('.ds-waive').checked)return '0';var v=tr.querySelector('.ds-fin').value;return v===''?'':String(Math.max(0,Math.round((+v)*100)/100));}
    function params(){return {x01:q('#dsDate').value,x02:q('#dsAcc').value,x03:chargeVal('A'),x04:chargeVal('L'),x05:chargeVal('V'),x10:st.mode.M,x11:st.mode.A,x12:st.mode.L,x13:st.mode.V};}
    function load(){var my=++st.seq;q('#dsBusy').hidden=false;
      W.call('DS_PREVIEW',params(),ITEMS).then(function(d){if(my!==st.seq)return;st.d=d;render();},function(m){if(my!==st.seq)return;W.toast(m,true);})
        .then(function(){if(my===st.seq)q('#dsBusy').hidden=true;});}
    function later(){clearTimeout(st.timer);st.timer=setTimeout(load,380);}
    // ---------- render
    function lineHtml(cls,label,val,minus){return '<li class="'+(cls||'')+(minus?' minus':'')+'"><span>'+label+'</span><b>'+(minus?'− ':'')+m2(val)+'</b></li>';}
    function render(){var d=st.d,T=d.totals,R=d.review,isDone=done();
      root.classList.toggle('is-done',isDone);
      if(isDone){var D=d.done||{};q('#dsDate').value=T.disbDate||'';q('#dsRef').value=D.paymentRef||'';
        q('#dsBank').innerHTML='<option>'+e(d.bank.bankName||'-')+'</option>';q('#dsAcc').innerHTML='<option>'+e(d.bank.account||'-')+'</option>';
        qa('.ds-in').forEach(function(x){x.disabled=true;});}
      // charges
      // with a product event rule only its charge components are shown
      var RC={A:'ADMIN_EXPENSE',L:'LEGAL_FEE',V:'VALUATION_FEE'};
      ['A','L','V'].forEach(function(k){row(k).hidden=!!d.ruleCharges&& d.ruleCharges.indexOf(RC[k])<0;});
      [['A','admin'],['L','legal'],['V','valuation']].forEach(function(x){var tr=row(x[0]),fr=+R[x[1]]||0,fin=+T[x[1]]||0,inp=tr.querySelector('.ds-fin');
        tr.querySelector('.ds-fr').textContent=m2(fr);inp.placeholder=fr.toFixed(2);
        if(isDone){inp.value=fin.toFixed(2);tr.querySelector('.ds-waive').checked=fin===0&& fr>0;}
        var w=tr.querySelector('.ds-waive').checked;tr.classList.toggle('is-off',w);tr.classList.toggle('is-ov',!w&& inp.value!==''&& Math.abs((+inp.value)-fr)>0.004);});
      // margin row and income recognition
      var M=row('M');M.querySelector('.ds-fr').textContent=m2(T.margin);M.querySelector('.ds-mfin').textContent=m2(T.margin);
      var TM=isDone&& d.treatment?d.treatment:st.mode;if(isDone&& d.treatment)st.mode={M:TM.M,A:TM.A,L:TM.L,V:TM.V};
      ['M','A','L','V'].forEach(function(k){qa('.ds-ch tr[data-ch="'+k+'"] .ds-tr button').forEach(function(b){b.setAttribute('aria-pressed',b.getAttribute('data-v')===TM[k]?'true':'false');});});
      M.querySelector('.ds-std').hidden=TM.M!=='IMMEDIATE';M.classList.toggle('is-warn',TM.M==='IMMEDIATE');
      q('#dsTrNote').innerHTML='<b>'+e(L('Income recognised on the disbursement date: ','الإيراد المُثبت في تاريخ الصرف: '))+m2((+T.marginImmediate||0)+(+T.feesImmediate||0))+'</b> '
        +e(L('(Cr 450102 margin income / 450103 fee income)','(دائن 450102 إيراد الهامش / 450103 إيراد الرسوم)'))+'<br><b>'+e(L('Deferred and released on the cheque dates: ','مؤجل ويُثبت في تواريخ الشيكات: '))
        +m2((+T.marginDeferred||0)+(+T.feesDeferred||0))+'</b> '+e(L('(Cr 120403 unearned margin / 120404 unearned fees)','(دائن 120403 هامش مؤجل / 120404 رسوم مؤجلة)'));
      // GL account of the bank
      var gl=d.bank||{};q('#dsGlLine').innerHTML='<span class="fa fa-book" aria-hidden="true"></span>'+e(L('Payment credited to GL account','يُقيد الدفع على حساب'))+' <b>'+e(gl.gl||'-')+'</b> '+e(gl.glName||'')
        +' <span class="ds-chip">'+e(L('Business line','خط الأعمال'))+' MORTGAGE</span>';
      // summary
      q('#dsWf').innerHTML=lineHtml('',e(L('Cheques face value','القيمة الاسمية للشيكات'))+' <small>('+T.cheques+')</small>',T.face)
        +lineHtml('',e(L('Discount','الخصم')),T.funding,true)
        +lineHtml('sub',e(L('Present value','القيمة الحالية')),T.pv)
        +lineHtml('',e(L('Margin','الهامش')),T.margin,true)
        +lineHtml('',e(L('Fees (admin, legal, valuation)','الرسوم (إدارية، قانونية، تقييم)')),T.fees,true)
        +lineHtml('sub',e(L('Paid to developer','المدفوع للمطور')),T.payable)
        +lineHtml('',e(L('Revenue','الإيراد')),(+T.face||0)-(+T.payable||0))
        +lineHtml('sub',e(L('Net profit (today\'s value)','صافي الربح بقيمة اليوم')),(+T.pv||0)-(+T.payable||0))
        +lineHtml('info',e(L('Recognised today','يُثبت اليوم')),(+T.marginImmediate||0)+(+T.feesImmediate||0))
        +lineHtml('info',e(L('Deferred to the cheque dates','مؤجل لتواريخ الشيكات')),(+T.marginDeferred||0)+(+T.feesDeferred||0))
        +(function(){var R=A('rate')===''||A('rate')==null?null:+A('rate'),pct=function(x){return (Math.round(x*100)/100)+'%';};return '<li class="info" style="display:block;border-bottom:0;font-size:12px;line-height:1.45">'+e(L('We assume the COF rate = PV rate'+(R==null?'':' ('+pct(R)+')')+', so the discount covers the cost of funds and the net profit equals margin + fees.','نفترض أن معدل تكلفة التمويل = معدل القيمة الحالية'+(R==null?'':' ('+pct(R)+')')+'، لذلك يغطي الخصم تكلفة التمويل، وصافي الربح = الهامش + الرسوم.'))+'</li>';})();
      q('#dsPay').textContent=m2(T.payable)+' '+cur;
      var diff=Math.round(((+T.payable)-(+R.price))*100)/100;
      q('#dsBoard').textContent=L('Board-approved price ','السعر المعتمد من المجلس ')+m2(R.price)+(Math.abs(diff)>0.004?' · '+L('difference ','الفرق ')+(diff>0?'+':'')+m2(diff):'');
      q('#dsTotal').style.background=(+T.payable)<=0?'linear-gradient(135deg,#8E2A1F,#C43325)':'';
      // alerts
      var h='';(d.blockers||[]).forEach(function(b){h+='<div class="ds-alert bad"><span class="fa fa-ban" aria-hidden="true"></span><span>'+e(b)+'</span></div>';});
      (d.warnings||[]).forEach(function(b){h+='<div class="ds-alert warn"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span><span>'+e(b)+'</span></div>';});
      if(!isDone&&!(d.blockers||[]).length)h+='<div class="ds-alert ok"><span class="fa fa-check-circle" aria-hidden="true"></span><span>'+e(L('Figures balance: interest equals the discount and both GL journals are balanced.','الأرقام متطابقة: الفائدة تساوي الخصم والقيدان متوازنان.'))+'</span></div>';
      q('#dsAlerts').innerHTML=h;
      if(isDone){var D2=d.done||{};q('#dsDone').innerHTML='<div class="ds-done"><span class="fxd-pill fxd-pill--ok">'+e(L('Completed','مكتمل'))+'</span>'
        +e(L('Disbursement ID: ','رقم الصرف: '))+'<b>'+e(D2.disbursementId==null?'-':String(D2.disbursementId))+'</b><br>'
        +e(L('TRAN_SET_ID: ','رقم مجموعة القيود: '))+'<b>'+e(D2.tranSetId||'-')+'</b><br>'
        +e(L('GL batch: ','دفعة القيود: '))+e(D2.glBatchNo||'-')+' · '+e(D2.glStatus||'')+'<br>'+e(D2.by||'')+' · '+e(D2.on||'')+(D2.comments?'<br><i>'+e(D2.comments)+'</i>':'')+'</div>';}
      // investor filter
      var sel=q('#dsAccSel'),o='<option value="ALL">'+e(L('All investors','كل المستثمرين'))+'</option>';
      (d.contracts||[]).forEach(function(c){o+='<option value="'+c.acc+'">'+e((W.ar()?c.nameAr:c.name)+' · #'+c.acc)+'</option>';});
      sel.innerHTML=o;if((d.contracts||[]).some(function(c){return String(c.acc)===st.acc;}))sel.value=st.acc;else st.acc='ALL';
      panel();steps();hint();
    }
    function nm(c){return W.ar()?c.nameAr:c.name;}
    function panel(){var d=st.d;if(!d)return;var P=q('#dsPanel'),h='',C={};(d.contracts||[]).forEach(function(c){C[c.acc]=c;});
      q('#dsAccSel').style.display=st.tab==='sch'||st.tab==='wal'?'':'none';
      if(st.tab==='sch'){var t={a:0,p:0,i:0,m:0,f:0},last=null;
        h='<div class="ds-scroll"><table class="wb-tbl"><thead><tr><th>#</th><th>'+e(L('Cheque','الشيك'))+'</th><th>'+e(L('Due date','الاستحقاق'))+'</th><th class="fxd-num">'+e(L('Amount','المبلغ'))+'</th>'
          +'<th class="fxd-num">'+e(L('Opening','الرصيد الافتتاحي'))+'</th><th class="fxd-num">'+e(L('Principal','الأصل'))+'</th><th class="fxd-num">'+e(L('Interest (discount)','الفائدة (الخصم)'))+'</th>'
          +'<th class="fxd-num">'+e(L('Closing','الرصيد الختامي'))+'</th><th class="fxd-num">'+e(L('Margin','الهامش'))+'</th><th class="fxd-num">'+e(L('Fees','الرسوم'))+'</th></tr></thead><tbody>';
        (d.lines||[]).forEach(function(r){if(st.acc!=='ALL'&& String(r.acc)!==st.acc)return;
          if(r.acc!==last){last=r.acc;var c=C[r.acc]||{};h+='<tr class="ds-grp"><td colspan="10">'+e(nm(c))+'<small>#'+r.acc+' · '+e(L('EIR','معدل العائد الفعلي'))+' '+(+c.eir||0).toFixed(4)+'% · '+e(L('PV','ق.ح'))+' '+m2(c.pv)+'</small></td></tr>';}
          t.a+=+r.amt||0;t.p+=+r.p||0;t.i+=+r.i||0;t.m+=+r.m||0;t.f+=+r.f||0;
          h+='<tr><td>'+r.line+'</td><td><span class="wb-code">'+e(r.no)+'</span></td><td>'+dt(r.due)+'</td><td class="fxd-num">'+m2(r.amt)+'</td><td class="fxd-num">'+m2(r.open)+'</td><td class="fxd-num">'+m2(r.p)+'</td>'
            +'<td class="fxd-num">'+m2(r.i)+'</td><td class="fxd-num">'+m2(r.close)+'</td><td class="fxd-num">'+m2(r.m)+'</td><td class="fxd-num">'+m2(r.f)+'</td></tr>';});
        h+='<tr class="ds-tot"><td colspan="3">'+e(L('Total','الإجمالي'))+'</td><td class="fxd-num">'+m2(t.a)+'</td><td></td><td class="fxd-num">'+m2(t.p)+'</td><td class="fxd-num">'+m2(t.i)+'</td><td></td><td class="fxd-num">'+m2(t.m)+'</td><td class="fxd-num">'+m2(t.f)+'</td></tr></tbody></table></div>'
          +'<div class="ds-note">'+e(L('Each cheque = principal + interest. Interest is the discount released by the effective interest method; for all investors it adds up to ','كل شيك = أصل + فائدة. الفائدة هي الخصم بطريقة معدل العائد الفعلي، وإجماليها لكل المستثمرين = '))+'<b>'+m2(d.totals.funding)+'</b>.</div>';}
      else if(st.tab==='inc'){var u={n:0,a:0,b:0,c:0,s:0};
        h='<div class="ds-scroll"><table class="wb-tbl"><thead><tr><th>'+e(L('Cheque date','تاريخ الشيك'))+'</th><th class="fxd-num">'+e(L('Cheques','الشيكات'))+'</th><th class="fxd-num">'+e(L('Discount','الخصم'))+'</th>'
          +'<th class="fxd-num">'+e(L('Margin','الهامش'))+'</th><th class="fxd-num">'+e(L('Fees','الرسوم'))+'</th><th class="fxd-num">'+e(L('Total income','إجمالي الإيراد'))+'</th><th>'+e(L('Posting','الترحيل'))+'</th></tr></thead><tbody>';
        (d.income||[]).forEach(function(r){u.n+=+r.n||0;u.a+=+r.funding||0;u.b+=+r.margin||0;u.c+=+r.fees||0;u.s+=+r.total||0;
          h+='<tr><td>'+dt(r.due)+'</td><td class="fxd-num">'+r.n+'</td><td class="fxd-num">'+m2(r.funding)+'</td><td class="fxd-num">'+m2(r.margin)+'</td><td class="fxd-num">'+m2(r.fees)+'</td><td class="fxd-num"><b>'+m2(r.total)+'</b></td>'
            +'<td><span class="fxd-pill '+(r.status==='POSTED'?'fxd-pill--ok':'fxd-pill--mute')+'">'+e(r.status==='POSTED'?L('Posted','مُرحّل'):L('Pending','قيد الانتظار'))+'</span></td></tr>';});
        h+='<tr class="ds-tot"><td>'+e(L('Total','الإجمالي'))+'</td><td class="fxd-num">'+u.n+'</td><td class="fxd-num">'+m2(u.a)+'</td><td class="fxd-num">'+m2(u.b)+'</td><td class="fxd-num">'+m2(u.c)+'</td><td class="fxd-num">'+m2(u.s)+'</td><td></td></tr></tbody></table></div>';
        h+='<div class="ds-note"><b>'+e(L('Month-end entry guide (category PP_INCOME)','دليل قيد نهاية الشهر (فئة PP_INCOME)'))+'</b><ul>';
        (d.release||[]).forEach(function(r){h+='<li>'+e(L('Dr ','مدين '))+'<b>'+e(r.from)+'</b> '+e(r.fromName||'')+' → '+e(L('Cr ','دائن '))+'<b>'+e(r.to)+'</b> '+e(r.toName||'')+'</li>';});
        h+='</ul></div>';}
      else if(st.tab==='gl'){var J={1:L('Journal 1 · Booking','القيد 1 · الإثبات'),2:L('Journal 2 · Payment to developer','القيد 2 · الدفع للمطور')};
        h='<div class="ds-note">'+e(L('Source PURCHASED_PORTFOLIO · category PP_DISBURSE · TRAN_SET_ID is created on Complete as PORTFOLIO-YEAR-NNNNN.','المصدر PURCHASED_PORTFOLIO · الفئة PP_DISBURSE · يُنشأ رقم TRAN_SET_ID عند الإكمال بصيغة المحفظة-السنة-رقم.'))
          +(done()&& d.done?' <b>'+e(d.done.tranSetId||'')+'</b>':'')
          +' · '+(d.glRule?e(L('Entries from product event rule ','القيود من قاعدة حدث المنتج '))+'<b>'+e(d.glRule)+'</b>':e(L('Fixed entries (the Disbursement step has no business event)','قيود ثابتة (خطوة الصرف بلا حدث أعمال)')))+'</div>';
        [1,2].forEach(function(j){var dr=0,cr=0,b='';
          (d.gl||[]).forEach(function(g){if(g.j!==j)return;dr+=+g.dr||0;cr+=+g.cr||0;
            b+='<tr><td><span class="wb-code">'+e(g.account)+'</span></td><td>'+e(g.name||'')+'<small>'+e(g.combination)+' · '+e(g.description)+'</small></td><td class="fxd-num">'+(+g.dr?m2(g.dr):'')+'</td><td class="fxd-num">'+(+g.cr?m2(g.cr):'')+'</td></tr>';});
          var ok=Math.abs(dr-cr)<0.005;
          h+='<div class="ds-jh"><span>'+e(J[j])+'</span><span class="'+(ok?'ds-ok':'ds-bad')+'"><span class="fa '+(ok?'fa-check-circle':'fa-times-circle')+'" aria-hidden="true"></span> '+e(ok?L('Balanced','متوازن'):L('Not balanced','غير متوازن'))+'</span></div>'
            +'<div class="wb-tbl-wrap"><table class="wb-tbl"><thead><tr><th>'+e(L('Account','الحساب'))+'</th><th>'+e(L('Name','الاسم'))+'</th><th class="fxd-num">'+e(L('Debit','مدين'))+'</th><th class="fxd-num">'+e(L('Credit','دائن'))+'</th></tr></thead><tbody>'
            +b+'<tr class="ds-tot"><td colspan="2">'+e(L('Total','الإجمالي'))+'</td><td class="fxd-num">'+m2(dr)+'</td><td class="fxd-num">'+m2(cr)+'</td></tr></tbody></table></div>';});}
      else if(st.tab==='crm'){var seen={};
        h='<div class="ds-note">'+e(L('Investors are matched to CRM clients by national ID. Investors not found are created as INDIVIDUAL clients when you complete.','يُطابق المستثمرون مع عملاء CRM برقم الهوية، ومن لا يوجد يُنشأ كعميل فرد عند الإكمال.'))+'</div>'
          +'<div class="wb-tbl-wrap"><table class="wb-tbl"><thead><tr><th>'+e(L('Investor','المستثمر'))+'</th><th>'+e(L('National ID','الرقم القومي'))+'</th><th class="fxd-num">'+e(L('Cheques','الشيكات'))+'</th><th>'+e(L('CRM client','عميل CRM'))+'</th></tr></thead><tbody>';
        (d.contracts||[]).forEach(function(c){if(seen[c.inv]){seen[c.inv].n+=c.n;return;}seen[c.inv]={c:c,n:c.n};});
        Object.keys(seen).forEach(function(k){var s=seen[k],c=s.c,badge=c.blacklisted==='Y'?'<span class="fxd-pill fxd-pill--bad">'+e(L('Blacklisted','قائمة سوداء'))+'</span>'
            :c.clientNew==='Y'?'<span class="fxd-pill fxd-pill--info">'+e(done()?L('Created','أُنشئ'):L('New','جديد'))+'</span>':'<span class="fxd-pill fxd-pill--ok">'+e(L('Existing','موجود'))+'</span>';
          h+='<tr><td><strong>'+e(nm(c))+'</strong><small>'+e(c.code||'')+'</small></td><td>'+e(c.nid||'-')+'</td><td class="fxd-num">'+s.n+'</td><td>'+badge+' <span class="wb-code">'+e(c.clientNo||'')+'</span>'+(c.clientId?' <small>ID '+c.clientId+'</small>':'')+'</td></tr>';});
        h+='</tbody></table></div>';}
      else{var n=0,s=0;
        h='<div class="ds-note">'+e(L('Every investor cheque goes to the cheque wallet: source MORTGAGE, status RECEIVED, location Company vault, with a RECEIVED event.','كل شيكات المستثمرين تُحفظ في محفظة الشيكات: المصدر MORTGAGE، الحالة مستلم، المكان خزينة الشركة، مع حدث استلام.'))+'</div>'
          +'<div class="ds-scroll"><table class="wb-tbl"><thead><tr><th>'+e(L('Cheque','الشيك'))+'</th><th>'+e(L('Bank','البنك'))+'</th><th>'+e(L('Due date','الاستحقاق'))+'</th><th class="fxd-num">'+e(L('Amount','المبلغ'))+'</th><th>'+e(L('Drawer / customer','الساحب / العميل'))+'</th><th>'+e(L('Wallet','المحفظة'))+'</th></tr></thead><tbody>';
        (d.lines||[]).forEach(function(r){if(st.acc!=='ALL'&& String(r.acc)!==st.acc)return;var c=C[r.acc]||{};n++;s+=+r.amt||0;
          h+='<tr><td><span class="wb-code">'+e(r.no)+'</span></td><td>'+e(r.bank||'-')+'</td><td>'+dt(r.due)+'</td><td class="fxd-num">'+m2(r.amt)+'</td><td>'+e(nm(c))+'<small>'+e(c.clientNo||'')+'</small></td>'
            +'<td>'+(r.wallet?'<span class="fxd-pill fxd-pill--ok">#'+r.wallet+' · '+e(L('Received','مستلم'))+'</span>':'<span class="fxd-pill fxd-pill--mute">'+e(L('On complete','عند الإكمال'))+'</span>')+'</td></tr>';});
        h+='<tr class="ds-tot"><td colspan="3">'+n+' '+e(L('cheques','شيك'))+'</td><td class="fxd-num">'+m2(s)+'</td><td colspan="2"></td></tr></tbody></table></div>';}
      P.innerHTML=h;qa('#dsTabs button').forEach(function(b){b.setAttribute('aria-pressed',b.getAttribute('data-tab')===st.tab?'true':'false');});}
    function ready(){var d=st.d;return d&&!done()&&!(d.blockers||[]).length&& q('#dsDate').value&& q('#dsAcc').value&&(q('#dsRef').value||'').trim()&&(+d.totals.payable)>0;}
    function steps(){var d=st.d,s=[!!(q('#dsDate').value&& q('#dsAcc').value&&(q('#dsRef').value||'').trim()),true,!!(d&&!(d.blockers||[]).length),done()];
      qa('#wbSteps .wb-step').forEach(function(el,i){el.classList.remove('done','now');if(s[i])el.classList.add('done');else if(!el.parentNode.querySelector('.now'))el.classList.add('now');el.querySelector('b').innerHTML=s[i]?'&#10003;':(i+1);});}
    function ring(){var aside=q('#dsWf')&&q('#dsWf').closest('aside');if(!aside)return;var c=q('#dsReady');
      if(done()){if(c)c.hidden=true;return;}
      if(!c){c=document.createElement('section');c.className='fxd-card';c.id='dsReady';
        c.innerHTML='<div class="fxd-card-h"><h2><span class="fa fa-check-square-o" aria-hidden="true"></span>'+e(L('Ready to complete','جاهز للإكمال'))+'</h2></div>'
          +'<div class="fxd-card-b"><div class="wb-ring-row"><div class="wb-ring" id="dsRing"><span><span><b id="dsRingN">0/5</b>'+e(L('steps','خطوات'))+'</span></span></div>'
          +'<p style="margin:0;color:var(--muted);font-size:13.5px" id="dsRingText"></p></div><ul class="wb-checks" id="dsChecks"></ul></div>';
        aside.insertBefore(c,aside.firstChild);
        c.addEventListener('click',function(ev){var li=ev.target.closest('li[data-go]');if(!li)return;var el=q('#'+li.getAttribute('data-go'));if(el){el.scrollIntoView({behavior:'smooth',block:'center'});setTimeout(function(){try{el.focus();}catch(x){}},300);}});}
      var d=st.d,blk=d&&(d.blockers||[]).length,items=[
        [!!q('#dsDate').value,L('Disbursement date','تاريخ الصرف'),L('Set the date of the payment.','حدد تاريخ الدفع.'),'dsDate'],
        [!!q('#dsAcc').value,L('Bank account','الحساب البنكي'),L('Choose the bank and account the payment leaves from.','اختر البنك والحساب الذي يخرج منه الدفع.'),'dsAcc'],
        [!!(q('#dsRef').value||'').trim(),L('Payment reference','مرجع الدفع'),L('Enter the bank transfer number.','أدخل رقم التحويل البنكي.'),'dsRef'],
        [!!d&&!blk&&(+d.totals.payable)>0,L('Figures checked','الأرقام سليمة'),blk?d.blockers[0]:L('The amount payable must be above zero.','يجب أن يكون المبلغ المستحق أكبر من صفر.'),null],
        [!!(q('#wbComment').value||'').trim(),L('Comment','التعليق'),L('Write a comment in the bar at the bottom.','اكتب تعليقًا في الشريط بالأسفل.'),'wbComment']],
        n=items.filter(function(x){return x[0];}).length,r=q('#dsRing');
      r.style.setProperty('--p',Math.round(n/5*100));r.style.setProperty('--ring',n===5?'var(--ok)':'var(--teal-600)');
      q('#dsRingN').textContent=n+'/5';
      q('#dsRingText').textContent=n===5?L('Everything is in place. Click Complete disbursement.','كل شيء جاهز. اضغط إكمال الصرف.'):L((5-n)+' step(s) left before you can complete. Click one to jump to it.','متبقي '+(5-n)+' خطوة قبل الإكمال. اضغط على أي منها للانتقال إليها.');
      q('#dsChecks').innerHTML=items.map(function(x,k){return '<li class="'+(x[0]?'ok':'')+'"'+(!x[0]&&x[3]?' data-go="'+x[3]+'"':'')+'><i>'+(x[0]?'✓':(k+1))+'</i><span>'+(!x[0]&&x[3]?'<a>'+e(x[1])+'</a>':e(x[1]))+(x[0]?'':'<small>'+e(x[2])+'</small>')+'</span></li>';}).join('');}
    function hint(){var b=q('#dsComplete'),cm=(q('#wbComment').value||'').trim(),d=st.d,miss=[];if(!b)return;
      if(!q('#dsDate').value)miss.push(L('disbursement date','تاريخ الصرف'));if(!q('#dsAcc').value)miss.push(L('bank account','الحساب البنكي'));
      if(!(q('#dsRef').value||'').trim())miss.push(L('payment reference','مرجع الدفع'));if(!cm)miss.push(L('comment','تعليق'));
      var blk=d&&(d.blockers||[]).length;b.disabled=!ready()||!cm;
      q('#wbHint').innerHTML=blk?'<b>'+e(L('Not ready:','غير جاهز:'))+'</b> '+e(d.blockers[0]):miss.length?e(L('Missing: ','ناقص: ')+miss.join(', ')):e(L('Ready to complete.','جاهز للإكمال.'));ring();}
    function sleep(ms){return new Promise(function(r){setTimeout(r,ms);});}
    function complete(){var d=st.d,cm=(q('#wbComment').value||'').trim();if(!ready()||!cm){hint();return;}
      var T=d.totals,acc=banks.filter(function(x){return String(x.a)===q('#dsAcc').value;})[0]||{};
      W.modal({icon:'fa-university',title:L('Complete the disbursement?','إكمال الصرف؟'),
        body:e(L('This is final and cannot be opened again. In one transaction the system creates the CRM clients, stores the cheques in the wallet, saves both schedules, posts the GL entries and completes the workflow step.','هذه خطوة نهائية ولا يمكن فتحها مرة أخرى. في معاملة واحدة يُنشئ النظام عملاء CRM ويحفظ الشيكات في المحفظة ويحفظ الجدولين ويرحّل القيود ويكمل خطوة سير العمل.')),
        summary:'<div><span>'+e(L('Pay to developer','الدفع للمطور'))+'</span><b>'+m2(T.payable)+' '+e(cur)+'</b></div><div><span>'+e(L('From','من'))+'</span><b>'+e((acc.bn||'')+' · '+(acc.an||''))+'</b></div>'
          +'<div><span>'+e(L('Income today','إيراد اليوم'))+'</span><b>'+m2((+T.marginImmediate||0)+(+T.feesImmediate||0))+'</b></div><div><span>'+e(L('Cheques to wallet','شيكات للمحفظة'))+'</span><b>'+T.cheques+'</b></div><div><span>'+e(L('New CRM clients','عملاء جدد'))+'</span><b>'+(T.clientsNew||0)+'</b></div>',
        ok:L('Complete disbursement','إكمال الصرف'),okClass:'fxd-btn--ok',cancel:L('Cancel','إلغاء'),
        onOk:function(){W.busy(true);var m=document.querySelector('#wbMask .wb-modal');
          m.querySelector('h2').textContent=L('Completing the disbursement...','جارٍ إكمال الصرف...');m.querySelector('.wb-m-sum').innerHTML='';
          var nb=m.querySelector('[data-m=no]'),yb=m.querySelector('[data-m=yes]');nb.style.display='none';yb.style.display='none';
          var restore=function(){nb.style.display='';yb.style.display='';};
          m.querySelector('p').innerHTML='<ul class="ds-prog"><li><i>&#10003;</i>'+e(L('CRM clients','عملاء CRM'))+'</li><li><i>&#10003;</i>'+e(L('Cheques to the wallet (company vault)','الشيكات إلى المحفظة (خزينة الشركة)'))+'</li>'
            +'<li><i>&#10003;</i>'+e(L('Repayment and income schedules','جداول السداد والإيراد'))+'</li><li><i>&#10003;</i>'+e(L('GL entries','القيود المحاسبية'))+'</li><li><i>&#10003;</i>'+e(L('Workflow step','خطوة سير العمل'))+'</li></ul>';
          var p=params();p.x06=q('#dsBank').value;p.x07=(q('#dsRef').value||'').trim();p.x08=cm;p.x09=apex.item('P217_TASK_ID').getValue();
          Promise.all([W.call('DS_COMPLETE',p,ITEMS),sleep(3200)]).then(function(r){var x=r[0]||{};
            [].forEach.call(m.querySelectorAll('.ds-prog li'),function(li){li.classList.add('ok');});
            return sleep(500).then(function(){yb.style.display='';
              W.modal({icon:'fa-check-circle',title:L('Disbursement completed','تم إكمال الصرف'),
                body:e(L('Transaction set ','مجموعة القيود '))+'<b>'+e(x.tranSetId||'')+'</b>. '+e(L('Taking you back to your inbox...','جارٍ العودة إلى صندوق المهام...')),
                ok:L('Go to inbox','الذهاب إلى الصندوق'),onOk:function(){window.location.href=A('inbox');}});
              setTimeout(function(){window.location.href=A('inbox');},3600);});
          },function(msg){restore();W.closeModal();W.toast(msg,true);load();});}});}
    // ---------- events
    function setMode(k,v){st.mode[k]=v;qa('.ds-ch tr[data-ch="'+k+'"] .ds-tr button').forEach(function(b){b.setAttribute('aria-pressed',b.getAttribute('data-v')===v?'true':'false');});later();}
    function ifrs(onYes){
      W.modal({tone:'warn ds-wide',icon:'fa-balance-scale',title:L('Recognise the margin immediately?','إثبات الهامش فوراً؟'),
        body:'<div class="ds-ifrs"><div class="ds-lead">'+e(L('The margin is the main profit of this deal. IFRS 9 and EAS 47 do not allow it to be booked as income on the disbursement date.','الهامش هو الربح الأساسي لهذه الصفقة، ولا يسمح معيارا IFRS 9 وEAS 47 بإثباته كإيراد في تاريخ الصرف.'))+'</div>'
          +'<h4>'+e(L('Why','السبب'))+'</h4><ul>'
          +'<li>'+e(L('The cheques are a financial asset measured at amortised cost (IFRS 9 section 4.1, EAS 47 Financial Instruments).','الشيكات أصل مالي يُقاس بالتكلفة المستهلكة (IFRS 9 القسم 4.1، ومعيار المحاسبة المصري 47 الأدوات المالية).'))+'</li>'
          +'<li>'+e(L('A margin earned on buying the asset is part of its effective interest rate (IFRS 9 B5.4.1 - B5.4.3). It is income for the time the money is outstanding, not for the purchase itself.','الهامش المكتسب عند شراء الأصل جزء من معدل العائد الفعلي له (IFRS 9 الفقرات B5.4.1 - B5.4.3)، فهو إيراد عن مدة بقاء الأموال وليس عن عملية الشراء ذاتها.'))+'</li>'
          +'<li>'+e(L('Income must follow the effective interest method over the expected life: here, the cheque dates (IFRS 9 5.4.1).','يجب إثبات الإيراد بطريقة معدل العائد الفعلي على العمر المتوقع، أي تواريخ الشيكات هنا (IFRS 9 الفقرة 5.4.1).'))+'</li></ul>'
          +'<h4>'+e(L('If you continue','إذا تابعت'))+'</h4><ul>'
          +'<li>'+e(L('Profit on the disbursement date is overstated and later months are understated.','يُضخَّم الربح في تاريخ الصرف ويُخفَّض في الأشهر التالية.'))+'</li>'
          +'<li>'+e(L('Auditors and the FRA will normally ask for the entry to be reversed and spread again.','عادةً ما يطلب المراجع الخارجي والهيئة العامة للرقابة المالية عكس القيد وإعادة توزيعه.'))+'</li>'
          +'<li>'+e(L('The choice is saved with the disbursement and shown as an exception.','يُحفظ هذا الاختيار مع الصرف ويظهر كاستثناء.'))+'</li></ul>'
          +'<div class="ds-rec"><span class="fa fa-lightbulb-o" aria-hidden="true"></span> '+e(L('Recommended: keep Pro-rate. Choose Immediate only with written approval from your auditor.','المقترح: الإبقاء على التوزيع الزمني، واختيار الإثبات الفوري فقط بموافقة كتابية من المراجع الخارجي.'))+'</div></div>',
        ok:L('Recognise immediately','إثبات فوري'),okClass:'fxd-btn--danger-solid',cancel:L('Keep pro-rate (recommended)','الإبقاء على التوزيع (مقترح)'),
        onOk:function(){W.closeModal();onYes();}});}
    root.addEventListener('click',function(e2){var tb=e2.target.closest('#dsTabs [data-tab]');if(tb){st.tab=tb.getAttribute('data-tab');panel();return;}
      var tbtn=e2.target.closest('.ds-tr button');if(tbtn){if(done()||tbtn.disabled)return;var k=tbtn.closest('tr').getAttribute('data-ch'),v=tbtn.getAttribute('data-v');
        if(st.mode[k]===v)return;if(k==='M'&& v==='IMMEDIATE'){ifrs(function(){setMode('M','IMMEDIATE');});return;}setMode(k,v);return;}
      if(e2.target.closest('#dsComplete'))complete();});
    root.addEventListener('change',function(e2){var t=e2.target;if(done())return;
      if(t.id==='dsBank'){fillAccs(false);later();return;}
      if(t.id==='dsAcc'||t.id==='dsDate'){later();return;}
      if(t.id==='dsAccSel'){st.acc=t.value;panel();return;}
      if(t.closest('.ds-waive')){var tr=t.closest('tr');tr.classList.toggle('is-off',t.checked);later();return;}});
    root.addEventListener('change',function(){try{hint();}catch(x){}});
    root.addEventListener('input',function(e2){var t=e2.target;if(t.closest('.ds-fin')){later();return;}if(t.id==='dsRef'||t.id==='wbComment'){steps();hint();}});
    // ---------- start
    fillBanks();fillAccs(false);q('#dsDate').value=A('today');
    var only={};banks.forEach(function(b){only[b.b]=1;});if(Object.keys(only).length===1){q('#dsBank').value=String(banks[0].b);fillAccs(false);}
    if(A('done')==='Y')qa('.ds-in').forEach(function(x){x.disabled=true;});
    load();W.flush();
  }
  if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',init);else init();
})();

(function(W){
  var toast=W.toast;
  function clean(m){m=String(m||'');var tech=!/ORA-20\d{3}/.test(m)&&/ORA-\d{5}/.test(m);
    m=m.replace(/ORA-06512[\s\S]*$/,'').replace(/^[\s\S]*?ORA-20\d{3}:\s*/,'').trim();return {text:m,tech:tech};}
  W.toast=function(msg,err){if(!err)return toast(msg);var mask=document.getElementById('wbMask');if(!mask)return toast(msg,err);
    var c=clean(msg),ar=W.ar();
    W.modal({tone:c.tech?'bad':'info',icon:c.tech?'fa-exclamation-circle':'fa-info-circle',
      title:c.tech?(ar?'تعذر إكمال العملية':'The action could not be completed'):(ar?'يرجى المراجعة':'Please check'),
      body:c.tech?W.esc(ar?'حدث خطأ في النظام. أرسل هذه التفاصيل إلى الدعم الفني:':'A system error occurred. Please send these details to support:')
        +'<br><small style="display:block;margin-top:8px;font-family:monospace;color:#8E2A1F;word-break:break-word">'+W.esc(c.text)+'</small>':W.esc(c.text),
      ok:ar?'حسناً':'OK',okClass:'fxd-btn--teal'});};
})(window.FXW);
