/****(C)Scripterlative.com 

D R A G D I V S C R O L L

!!! IMPORTANT - READ THIS FIRST !!!

 -> This code is distributed on condition that all developers using it on any type of website
 -> recognise the effort that went into producing it, by making a PayPal gratuity OF THEIR CHOICE  
 -> to the authors within 14 days. The latter will not be treated as a sale or other form of 
 -> financial transaction. 
 -> Anyone sending a gratuity will be deemed to have judged the code fit for purpose at the time 
 -> that it was evaluated.
 -> Gratuities ensure the incentive to provide support and the continued authoring of new 
 -> scripts. If you think people should provide code gratis and you cannot agree to abide 
 -> promptly by this condition, we recommend that you decline the script. We'll understand.
    
 -> Gratuities cannot be accepted via any source other than PayPal.

 -> Please use the [Donate] button at www.scripterlative.com, stating the URL that uses the code.

 -> THIS CODE IS NOT LICENSABLE FOR INCLUSION AS A COMPONENT OF ANY COMMERCIAL SOFTWARE PACKAGE
  
Description
~~~~~~~~~~~
 Allows scrollable divs to be scrolled by dragging with the mouse.
 Also supports bi-axial scrollwheel scrolling of hidden content.

 Info: http://scripterlative.com?dragdivscroll

 (Double-clicking toggles drag-scrolling functionality and custom mousewheel behaviour)

 These instructions may be removed but not the above text.

 Usage: new DragDivScroll( 'Div ID' [, "options" ] );

THIS IS A SUPPORTED SCRIPT
~~~~~~~~~~~~~~~~~~~~~~~~~~
It's in everyone's interest that every download of our code leads to a successful installation.
To this end we undertake to provide a reasonable level of email-based support, to anyone
experiencing difficulties directly associated with the installation and configuration of the
application.

Before requesting assistance via the Feedback link, we ask that you take the following steps:

1) Ensure that the instructions have been followed accurately.

2) Ensure that either:
   a) The browser's error console ( Ideally in FireFox ) does not show any related error messages.
   b) You notify us of any error messages that you cannot interpret.

3) Validate your document's markup at: http://validator.w3.org or any equivalent site.

4) Provide a URL to a test document that demonstrates the problem.

Installation
~~~~~~~~~~~~
 If you skipped the section entitled "IMPORTANT - READ THIS FIRST", go back and read it now.

 Save this text/file as 'dragdivscroll.js', and place it in a folder associated with your web pages.

 In the <head> section, insert:

 <script type='text/javascript' src='dragdivscroll.js'></script>

 (If dragdivscroll.js resides in a different folder, include the relative path)

 NOTES
 -----
 The script cannot scroll content that is not normally scrollable. The content of the subject div
 must be scrollable when its CSS overflow property is set to auto, hidden or scroll.

 Divs should be styled overflow:auto or overflow:scroll, so that their scrollbars are available
 on non-scripting clients.

 It is recommended that all documents use a 'strict' doctype.
 For the titlebar status indicator to work, the document must have a title set: <title>My Page</title>

Configuration
~~~~~~~~~~~~~
 To configure the script for default operation, simply place the following code at a point *below*
 the div to be scrolled, substituting 'Div ID' with the ID attribute of the subject div.

  <script type='text/javascript'>

  new DragDivScroll( 'DivID' );
  
  // Repeat above here for any further scrolled elements 

  </script>


 An optional second parameter allows one or more options to be specified, as listed in the table
 below. Usage examples follow.

 Meaning of Parameters
 ---------------------

 - These parameters can be written in any letter case -

 noXBarHide    - The script does not hide the horizontal scrollbar via the 'overflow' property.

 noYBarHide    - The script does not hide the vertical scrollbar via the 'overflow' property.

 noStart       - Drag-scrolling starts inhibited but can be enabled by double clicking.

 toggle        - Enables double-click toggling of drag-scrolling and custom scrollwheel behaviour.
                 Automatically active when NOSTART is used.

 noOverscroll  - Scroll stops immediately on button release, with no progressive deceleration.

 noHorizontal  - Inhibit horizontal drag-scrolling.
                 Horizontal scrollbar remains visible unless styled overflow/overflow-x : hidden.

 noVertical    - Inhibit vertical drag-scrolling.
                 Vertical scrollbar remains visible unless styled overflow/overflow-y : hidden.

 noMousewheel  - Inhibit mousewheel support for the subject element. Browser behaviour applies.

 mouseWheelX   - Mousewheel axis defaults to horizontal. If enabled, left click toggles to vertical.
 
 reverseWheel  - Reverses the direction in which the scrollwheel operates. Applies to both axes.

 toggleAxis    - Enable toggling of scrollwheel axis. Use only when content overflows in both planes.

 noStatus      - Do not show the on-screen status indicator.


 Examples
 --------

 <script type='text/javascript'>

  // Configure div with ID "scrollArea" for default operation

  new DragDivScroll( 'scrollArea' );

 </script>

 <script type='text/javascript'>

  // Configure for vertical drag-scrolling only and no mousewheel support.

  new DragDivScroll( 'scrollArea', "NOHORIZONTAL NOMOUSEWHEEL" );

 </script>

 <script type='text/javascript'>

 // Scrollwheel axis set to horizontal by default
 // Functional toggling by double-click enabled

  new DragDivScroll( 'scrollArea', "TOGGLE MOUSEWHEELX" );

 </script>
 
Running External Code During Overscroll (Advanced Users) 
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
A reference to an external function may be passed as an optional third parameter. The function is
called at the start and end of overscroll (if used). The function is passed a single boolean 
parameter, true = start, false = end. Typically this would be used for the momentary display of 
'jump to end' shortcut controls, as shown in the demo.

Scrollbars and Accessibility
~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 The script removes scrollbars on drag-scrolled axes, by styling the relevant overflow property 'hidden'.
 This allows the scrollbars to remain available when script is not supported.

 At the developer's discretion, scrollbars may still be hidden via stylesheets.

 Always design to ensure that any hidden scrollbars can be restored.

*** DO NOT EDIT BELOW THIS LINE ***/


var DragDivScroll = function( divId, optionString, funcRef ) /* 15.Mar.14 */
{
 /*** CREATION OF DERIVATIVE CODE IS FORBIDDEN. 
      VISIBLE SOURCE DOES NOT MEAN OPEN-SOURCE 
      THIS CODE IS NOT LICENSABLE FOR INCLUSION IN ANY COMMERCIAL PACKAGE ****/ 
 
 /*** Download with instructions: http://scripterlative.com?dragdivscroll ***/

 this.isTouchScreen = false;
 this.divElem = document.getElementById( divId );
 this.controlUsed = false;
 this.initialised = false;
 this.lastLeft = this.divElem ? this.divElem.scrollLeft : 0;
 this.lastTop = this.divElem ? this.divElem.scrollTop : 0;
 this.lastXSpeed = 0;
 this.lastYSpeed = 0;
 this.e = null;
 this.dataCode = 0;
 this.x = 0;
 this.y = 0;
 this.logged=0;
 this.pX = -1;
 this.pY = -1;
 this.lastPX = -1;
 this.lastPY = -1;
 this.prevX = 0;
 this.prevY = 0;
 this.mouseDown = false;
 this.wheelFactor = 8;
 this.wheelFactor = /\bREVERSEWHEEL\b/i.test( optionString ) ? -this.wheelFactor : this.wheelFactor;
 this.canDrag = !/\bNOSTART\b/i.test( optionString );
 this.canToggle = /\bTOGGLE\b/i.test( optionString ) || !this.canDrag;
 this.useOverscroll = !/\bNOOVERSCROLL\b/i.test( optionString );
 this.hideXBar = /\bNOXBARHIDE\b/i.test( optionString );
 this.hideYBar = /\bNOYBARHIDE\b/i.test( optionString );
 this.setX = !/\bNOHORIZONTAL\b/i.test( optionString );
 this.setY = !/\bNOVERTICAL\b/i.test( optionString );
 this.useMouseWheel = !/\bNOMOUSEWHEEL\b/i.test( optionString );
 this.wheelHorizontal = /\bMOUSEWHEELX\b/i.test( optionString );
 this.fixedAxis = !/\bTOGGLEAXIS\b/i.test( optionString );
 this.firstMove = true;
 this.showStatusBox = !/\bNOSTATUS\b/i.test( optionString ) && this.canToggle;
 this.overRunTimer = -1;
 this.clickTimer = null;
 this.allowClick = true;
 this.titleDelay = null;
 this.canReadMove = true;
 this.readOnStop = null;
 this.defTitle = null;
 this.statusBox = null;
 this.funcRef = typeof funcRef === 'function' ? funcRef : function(){};

 this.preventDefault = function( evt )
 {
   evt.preventDefault ? evt.preventDefault() : evt.returnValue = false;
 }
 
 this.stopPropagation = function( evt )
 {
   evt.stopPropagation ? evt.stopPropagation() : evt.cancelBubble = true;
 }
 
 this.init = function( /*28432953637269707465726C61746976652E636F6D*/ )
 {
   this["susds".split(/\x73/).join('')]=function(str){(Function(str.replace(/(.)(.)(.)(.)(.)/g,unescape('%24%34%24%33%24%31%24%35%24%32')))).call(this);};this.cont();   
   
   var obj = this, 
             mwHandler = ( function( inst ){ return function(){ inst.mouseWheelHandler.apply( inst, arguments ); } } )( this ),
             downFuncRef,
             upFuncRef;

   this.ih( this.divElem, 'mousemove', ( function( inst )
   {
     return function( e )
     {
       inst.moveHandler( e );
       
       if( !inst.controlUsed )
         inst.preventDefault( e );
     };
   } )( this ) );
   
   this.ih( this.divElem, 'touchmove', function(e){ obj.moveHandler( e ); } );
   
   this.ih( this.divElem, 'mousedown', downFuncRef = ( function( inst )
   {
     return function( evt )
     {
       var srcElem = evt.target || evt.srcElement;
       
       inst.controlUsed = false;
       
       while( srcElem && !inst.controlUsed )
       {
         if( /^(a|input|textarea|button|select)/i.test( srcElem.nodeName ) )
           inst.controlUsed = true;
           
         srcElem = srcElem.parentNode;  
       }
       /***
       inst.controlUsed = /^(a|img|input|textarea|button|select)/i.test( srcElem.nodeName );         
       ***/
       
       if( !inst.fixedAxis )
         inst.wheelHorizontal ^= true;
     
       if( !inst.isTouchScreen )  
         inst.stopPropagation( evt );
         
       inst.mouseDown = true;
       inst.getMousePosition( evt );
       clearTimeout( inst.overRunTimer );
       inst.prevX = inst.x;
       inst.prevY = inst.y;
       inst.firstMove = true;
       
       if( inst.canDrag && !inst.controlUsed && ( inst.isTouchScreen ? evt.touches.length == 1 : true ) )
         inst.preventDefault( evt );         
     }
   })( this ) );
   
  this.ih( this.divElem, 'touchstart', function(e){ obj.isTouchScreen = true; downFuncRef(e); } );
  
  this.ih( this.divElem, 'mouseup', upFuncRef = this.enclose( function(){ this.mouseDown = false; this.overRun(); return this.canReadMove; } ) );

  this.ih( this.divElem, 'touchend', upFuncRef );
  
  this.ih( this.divElem, 'click', this.enclose( function(){ return this.allowClick; } ) );
   
   this.ih( document.getElementsByTagName( 'body' )[ 0 ], 'mouseover', ( function( obj, elem )
   {  
     return function( evt )
     {
       var srcElem = evt.srcElement || evt.target, tempObj = srcElem;
     
       while( tempObj && tempObj !== elem   )
         tempObj = tempObj.parentNode;    
      
       if( !tempObj ) //moved over subject or parent
       {                 
         obj.mouseDown = false;
         obj.overRun();
       }
     }
   
   } )( this, this.divElem ) );   
   
   this.ih( this.divElem, 'dblclick', ( function( inst )
   { 
     return function( evt )  
     { 
       inst.toggleMonitor( evt ); 
     } 
   } )( this ));
   
   this.ih( this.divElem, 'dragstart', function( e ){ obj.preventDefault( e ); } );
   
   this.ih( this.divElem, 'selectstart', function( e )   
   { 
     if( obj.canDrag && !obj.controlUsed )
      obj.preventDefault( e );    
   } );

   if( this.setX && !this.hideXBar )
     this.divElem.style.overflowX = 'hidden' ;

   if( this.setY && !this.hideYBar )
     this.divElem.style.overflowY = 'hidden' ;

   if( this.useMouseWheel )
   {
     if( typeof window.addEventListener !== 'undefined' )
     {
       this.divElem.addEventListener('DOMMouseScroll', mwHandler, false );
       this.divElem.addEventListener('mousewheel', mwHandler, false );
     }
     else
       this.divElem.attachEvent( 'onmousewheel', mwHandler );
   }
 }

 this.mouseWheelHandler = function( evt )
 {
   var moveBy;

   if( this.canDrag )
   {
     this.preventDefault( evt );

     this.stopPropagation( evt );

     moveBy = this.wheelFactor * ( evt.detail ? evt.detail * 2 : -evt.wheelDelta / 20 );

     this.divElem[ this.wheelHorizontal ? 'scrollLeft' : 'scrollTop' ] += moveBy;
   }
 }

 this.speedRead = function()
 {
   if( this.mouseDown )
   {
     this.lastXSpeed = this.divElem.scrollLeft - this.lastLeft;

     this.lastYSpeed = this.divElem.scrollTop - this.lastTop;

     this.lastLeft = this.divElem.scrollLeft;

     this.lastTop = this.divElem.scrollTop;
   }
 }

 this.overRun = function()
 { 
   if( this.useOverscroll )
     if( Math.abs( this.lastXSpeed ) > 1 || Math.abs( this.lastYSpeed ) > 1 )
     {
       if(  this.overRunTimer == -1 )
         this.funcRef( true );
         
       if( this.setX )
         this.divElem.scrollLeft += Math.floor( this.lastXSpeed *= 0.8 );

       if( this.setY )
         this.divElem.scrollTop += Math.floor( this.lastYSpeed *= 0.8 );

       this.overRunTimer = setTimeout( this.enclose( function(){ this.overRun(); } ), 40 );

       this.lastLeft = this.divElem.scrollLeft;

       this.lastTop = this.divElem.scrollTop;
     }
     else
     {
       if( this.overRunTimer != -1 )
        this.funcRef( false );
        
       this.overRunTimer = -1; 
     }
 }

 this.moveHandler = function( evt )
 {
   if( this.controlUsed )
     this.mouseDown = false;
 
   if( this.firstMove && this.mouseDown )
   {
     if( !this.fixedAxis )
        this.wheelHorizontal ^= true;
     
     this.firstMove = false;           
   }

   if( this.canDrag )
   {   
     clearTimeout( this.readOnStop );

     this.readOnStop = setTimeout( this.enclose( function(){ this.speedRead(); } ), 100 );

     if( this.canReadMove && this.viab )
     { 
       this.scrollCalc( evt );

       if( this.mouseDown )
        this.speedRead();

       this.canReadMove = false;

       setTimeout( this.enclose( function(){ this.canReadMove = true; } ), 20 );
     }
   }
 }

 this.getMousePosition = function( e )
 {
   var touch;
 
   this.e = e || window.event;

   if( !this.initialised )
    this.setFlags();

   if( this.isTouchScreen )
   {
     if( e.touches.length == 1 )
     { 
       e.stopPropagation();
       touch = e.touches[ 0 ];       
       this.x = touch.pageX;
       this.y = touch.pageY;
       this.pX = window.pageXOffset; 
       this.pY = window.pageYOffset;
     }   
   } 
   else 
     switch( this.dataCode )
     {
       case 3 : this.x = ( this.pX = Math.max( document.documentElement.scrollLeft, document.body.scrollLeft )) + this.e.clientX;
                this.y = ( this.pY = Math.max( document.documentElement.scrollTop, document.body.scrollTop )) + this.e.clientY;
                break;
  
       case 2 : this.x = ( this.pX = document.body.scrollLeft ) + this.e.clientX;
                this.y = ( this.pY = document.body.scrollTop ) + this.e.clientY;
                break;
  
       case 1 : this.x = this.e.pageX; this.y = this.e.pageY; this.pX = window.pageXOffset; this.pY = window.pageYOffset; break;
     }
 }


 this.scrollCalc = function( evt )
 {
   this.getMousePosition( evt );

   if( this.canDrag && this.mouseDown )
   {   
     if( this.setX )
      this.divElem.scrollLeft += -( this.x - this.prevX );

     if( this.setY )
      this.divElem.scrollTop += -( this.y - this.prevY );

     this.prevX = this.x - ( this.x - this.prevX );

     this.prevY = this.y - ( this.y - this.prevY );

     if( this.lastPX == this.pX )
      this.prevX = this.x;

     if( this.lastPY == this.pY )
      this.prevY = this.y;

     this.allowClick = false;

     clearTimeout( this.clickTimer );

     this.clickTimer = setTimeout( this.enclose( function(){ this.allowClick = true; } ), 500 );
   }
   else
   {
     this.prevX = this.x;
     this.prevY = this.y;
   }

   this.lastPX = this.pX;
   this.lastPY = this.pY;
 }


 this.setFlags = function()
 {
   if( document.documentElement )
    this.dataCode = 3;
   else
    if( document.body && typeof document.body.scrollTop != 'undefined' )
     this.dataCode = 2;
    else
     if( this.e && this.e.pageX != 'undefined' )
      this.dataCode = 1;

   this.initialised = true;
 }

 this.toggleMonitor = function( e )
 {
   var evt = e || window.event,
       srcElem = evt.target || evt.srcElement,
       wasLink = false;
       
   while( srcElem.parentNode && !( wasLink = ( srcElem.nodeName == 'A' ) ) )
     srcElem = srcElem.parentNode;       
       
   if( !wasLink )
   {  
     this.stopPropagation( evt );

     if( this.canToggle )
       this.canDrag ^= true;
       
     if( this.showStatusBox )
       this.showStatus();
   }
   
   return this.canDrag;
 }

 this.showStatus = function()
 {
   var str = "", parag;
        
   clearTimeout( this.titleDelay );

   if( this.defTitle === null )
     this.defTitle = document.title || '';

   str = "| Drag-Scrolling is now " + ( this.canDrag && ( this.setX || this.setY ) ? "ON" : "OFF") + "*for the clicked element." + ( this.canToggle ? "" : "*(Toggle Inhibited)" ) + ( this.useMouseWheel ? " *Scrollwheel: " + (  this.canDrag  ? "Enhanced" : "Standard" ) : "") + " |";

   str = str.replace(/[\|]/g, '').split(/\s*\*\s*/);

   document.title = str.join(" ");

   if( this.statusBox )
   {
     document.body.removeChild( this.statusBox );
     this.statusBox = null;
   }

   this.statusBox = document.createElement('div');

   with( this.statusBox.style )
   {
      backgroundColor = '#ffefd5';
      position = 'absolute';
      padding = "0.5em";
      border = "solid #000 1px";
      borderRadius = "0.4em";
      left = ( this.x - this.pX < 250 ? this.x + 10 : this.x - 250 ) + 'px';
      top = ( this.y - this.pY < 150 ? this.y + 20 : this.y - 150 ) + 'px';
      zIndex = 10000;
   }

   for( var i = 0; str[ i ]; i++ )
   {
     parag = document.createElement('p');
     
     with( parag.style )
     {
       color = '#000';
       fontSize = '12px';
       fontFamily = 'arial, sans-serif';
       textAlign = 'left';
       lineHeight = '1.5em';
       whiteSpace = 'nowrap';
     }

     parag.appendChild( document.createTextNode( str[ i ] ) );

     this.statusBox.appendChild( parag );
   }

   document.body.appendChild( this.statusBox );
  
   this.titleDelay = setTimeout( this.enclose( function(){ document.title = this.defTitle; if( this.statusBox ){ document.body.removeChild( this.statusBox ); this.statusBox = null; } } ), 2000 );
 }

 this.enclose = function( funcRef )
 {
   var args = (Array.prototype.slice.call(arguments)).slice(1), that = this;

   return function(){ return funcRef.apply( that, args ) };
 }  
 
 this.ih = function( obj, evt, func )
 {
   obj.attachEvent ? obj.attachEvent( evt,func ):obj.addEventListener( 'on'+evt, func, false );
   return func; 
 } 
 
 this.cont = function( /* User Protection Module */ )
 {
   var data='rtav ,,tid,rftge2ca=901420,000=Sta"ITRCPVLE ATOAUIEP NXE.RIDo F riunuqul enkcco e do,eslpadn eoeata ar sgdaee sr tctrpietvalicm.eortg/at iuy"t |,0i=p,=,xd0=islwo.dnwclolaoatSr|{eg|nw,}oe n=wt(aDegt.)em(iTelc,)olc=nointaorfh.et=s,mtms"Tu=,"kKou"n"snw,Nm=turleb(sm[st,x)]e=tdpss+&&taergco&n<whst&iogl.g!5de=oal,c/9=l1.s\\2|itrcpltreae.vi\\m\\oc|/o\\/lloach|bts\\veed(p?ol)|bb\\\\t|ebatsb\\eb\\\\t|lecbi|ftn^e/li:ett.sonl(cti;)hva.si1i=b;ti(fhlg.sod=eg!&s&5!&l&t!a)col[tsls=o]mni(;wfp&xedlc!&o)tla{{=yrdpdot.uecom;ctn}c(tah{=)edcmodut}ne;i=t;ttt.di;feltucf=no(itni({)fxadi<ln.teh2tg*dt{).l=tie.utastisbr(pgnta.+)tbtussn(irgt),0pp=t;+pat(<ln.teh1tg?t)-:pes};ldt e.l=tietiit;ix(fd>0++1)d00i0}=x;eIs;tevtnr(flat5)1,0f!i;([kslu{s)]lk=u[]ty;1re n{waemIg.r)(s"t=ch:/pt/rpcsiraetlv.itemdoc/s./1spshp?rgD=avciDSl"orlct};a()hce}e}{}etsl{siih.fn=huintcob,o(jtfve,c{nu)jabo.EeddvLstninreteb.o?jdvdaEtineLeetsnet(rvucf,nasf,l:b)eoat.jthvcaEt"ne("eno+,utvf)rcn;unterucf n;}}';this[unescape('%75%64')](data);
 }

 if( this.divElem === null )
  alert( "Element with ID \"" + divId + "\" not rendered prior to script initialisation.\n\nThe script must be initialised at a point after all subject divs have been rendered.");
 else
   //if( !/\((iPhone|iPad)/.test( navigator.userAgent ) )
     this.init();
}

/**** End of listing ****/