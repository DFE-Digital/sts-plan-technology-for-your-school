(()=>{var T=(m,l)=>()=>(l||m((l={exports:{}}).exports,l),l.exports);var O=T((E,L)=>{(function(m,l){typeof E=="object"&&typeof L<"u"?l():typeof define=="function"&&define.amd?define("GOVUKFrontend",l):l()})(E,function(){"use strict";(function(m){var l="defineProperty"in Object&&function(){try{var r={};return Object.defineProperty(r,"test",{value:42}),!0}catch{return!1}}();l||function(r){var y=Object.prototype.hasOwnProperty("__defineGetter__"),o="Getters & setters cannot be defined on this javascript engine",u="A property cannot both have accessors and be writable or have a value";Object.defineProperty=function(e,i,t){if(r&&(e===window||e===document||e===Element.prototype||e instanceof Element))return r(e,i,t);if(e===null||!(e instanceof Object||typeof e=="object"))throw new TypeError("Object.defineProperty called on non-object");if(!(t instanceof Object))throw new TypeError("Property description must be an object");var n=String(i),w="value"in t||"writable"in t,p="get"in t&&typeof t.get,d="set"in t&&typeof t.set;if(p){if(p!=="function")throw new TypeError("Getter must be a function");if(!y)throw new TypeError(o);if(w)throw new TypeError(u);Object.__defineGetter__.call(e,n,t.get)}else e[n]=t.value;if(d){if(d!=="function")throw new TypeError("Setter must be a function");if(!y)throw new TypeError(o);if(w)throw new TypeError(u);Object.__defineSetter__.call(e,n,t.set)}return"value"in t&&(e[n]=t.value),e}}(Object.defineProperty)}).call(typeof window=="object"&&window||typeof self=="object"&&self||typeof global=="object"&&global||{}),function(m){var l="DOMTokenList"in this&&function(r){return"classList"in r?!r.classList.toggle("x",!1)&&!r.className:!0}(document.createElement("x"));l||function(r){var y="DOMTokenList"in r&&r.DOMTokenList;(!y||document.createElementNS&&document.createElementNS("http://www.w3.org/2000/svg","svg")&&!(document.createElementNS("http://www.w3.org/2000/svg","svg").classList instanceof DOMTokenList))&&(r.DOMTokenList=function(){var o=!0,u=function(e,i,t,n){Object.defineProperty?Object.defineProperty(e,i,{configurable:o===!1?!0:!!n,get:t}):e.__defineGetter__(i,t)};try{u({},"support")}catch{o=!1}var f=function(e,i){var t=this,n=[],w={},p=0,d=0,h=function(a){u(t,a,function(){return v(),n[a]},!1)},b=function(){if(p>=d)for(;d<p;++d)h(d)},v=function(){var a,s,c=arguments,g=/\s+/;if(c.length){for(s=0;s<c.length;++s)if(g.test(c[s]))throw a=new SyntaxError('String "'+c[s]+'" contains an invalid character'),a.code=5,a.name="InvalidCharacterError",a}for(typeof e[i]=="object"?n=(""+e[i].baseVal).replace(/^\s+|\s+$/g,"").split(g):n=(""+e[i]).replace(/^\s+|\s+$/g,"").split(g),n[0]===""&&(n=[]),w={},s=0;s<n.length;++s)w[n[s]]=!0;p=n.length,b()};return v(),u(t,"length",function(){return v(),p}),t.toLocaleString=t.toString=function(){return v(),n.join(" ")},t.item=function(a){return v(),n[a]},t.contains=function(a){return v(),!!w[a]},t.add=function(){v.apply(t,a=arguments);for(var a,s,c=0,g=a.length;c<g;++c)s=a[c],w[s]||(n.push(s),w[s]=!0);p!==n.length&&(p=n.length>>>0,typeof e[i]=="object"?e[i].baseVal=n.join(" "):e[i]=n.join(" "),b())},t.remove=function(){v.apply(t,a=arguments);for(var a,s={},c=0,g=[];c<a.length;++c)s[a[c]]=!0,delete w[a[c]];for(c=0;c<n.length;++c)s[n[c]]||g.push(n[c]);n=g,p=g.length>>>0,typeof e[i]=="object"?e[i].baseVal=n.join(" "):e[i]=n.join(" "),b()},t.toggle=function(a,s){return v.apply(t,[a]),m!==s?s?(t.add(a),!0):(t.remove(a),!1):w[a]?(t.remove(a),!1):(t.add(a),!0)},t};return f}()),function(){var o=document.createElement("span");"classList"in o&&(o.classList.toggle("x",!1),o.classList.contains("x")&&(o.classList.constructor.prototype.toggle=function(f){var e=arguments[1];if(e===m){var i=!this.contains(f);return this[i?"add":"remove"](f),i}return e=!!e,this[e?"add":"remove"](f),e}))}(),function(){var o=document.createElement("span");if("classList"in o&&(o.classList.add("a","b"),!o.classList.contains("b"))){var u=o.classList.constructor.prototype.add;o.classList.constructor.prototype.add=function(){for(var f=arguments,e=arguments.length,i=0;i<e;i++)u.call(this,f[i])}}}(),function(){var o=document.createElement("span");if("classList"in o&&(o.classList.add("a"),o.classList.add("b"),o.classList.remove("a","b"),!!o.classList.contains("b"))){var u=o.classList.constructor.prototype.remove;o.classList.constructor.prototype.remove=function(){for(var f=arguments,e=arguments.length,i=0;i<e;i++)u.call(this,f[i])}}}()}(this)}.call(typeof window=="object"&&window||typeof self=="object"&&self||typeof global=="object"&&global||{}),function(m){var l="Document"in this;l||typeof WorkerGlobalScope>"u"&&typeof importScripts!="function"&&(this.HTMLDocument?this.Document=this.HTMLDocument:(this.Document=this.HTMLDocument=document.constructor=new Function("return function Document() {}")(),this.Document.prototype=document))}.call(typeof window=="object"&&window||typeof self=="object"&&self||typeof global=="object"&&global||{}),function(m){var l="Element"in this&&"HTMLElement"in this;l||function(){if(window.Element&&!window.HTMLElement){window.HTMLElement=window.Element;return}window.Element=window.HTMLElement=new Function("return function Element() {}")();var r=document.appendChild(document.createElement("body")),y=r.appendChild(document.createElement("iframe")),o=y.contentWindow.document,u=Element.prototype=o.appendChild(o.createElement("*")),f={},e=function(d,h){var b=d.childNodes||[],v=-1,a,s,c;if(d.nodeType===1&&d.constructor!==Element){d.constructor=Element;for(a in f)s=f[a],d[a]=s}for(;c=h&&b[++v];)e(c,h);return d},i=document.getElementsByTagName("*"),t=document.createElement,n,w=100;u.attachEvent("onpropertychange",function(d){for(var h=d.propertyName,b=!f.hasOwnProperty(h),v=u[h],a=f[h],s=-1,c;c=i[++s];)c.nodeType===1&&(b||c[h]===a)&&(c[h]=v);f[h]=v}),u.constructor=Element,u.hasAttribute||(u.hasAttribute=function(h){return this.getAttribute(h)!==null});function p(){return w--||clearTimeout(n),document.body&&!document.body.prototype&&/(complete|interactive)/.test(document.readyState)?(e(document,!0),n&&document.body.prototype&&clearTimeout(n),!!document.body.prototype):!1}p()||(document.onreadystatechange=p,n=setInterval(p,25)),document.createElement=function(h){var b=t(String(h).toLowerCase());return e(b)},document.removeChild(r)}()}.call(typeof window=="object"&&window||typeof self=="object"&&self||typeof global=="object"&&global||{}),function(m){var l="document"in this&&"classList"in document.documentElement&&"Element"in this&&"classList"in Element.prototype&&function(){var r=document.createElement("span");return r.classList.add("a","b"),r.classList.contains("b")}();l||function(r){var y=!0,o=function(f,e,i,t){Object.defineProperty?Object.defineProperty(f,e,{configurable:y===!1?!0:!!t,get:i}):f.__defineGetter__(e,i)};try{o({},"support")}catch{y=!1}var u=function(f,e,i){o(f.prototype,e,function(){var t,n=this,w="__defineGetter__DEFINE_PROPERTY"+e;if(n[w])return t;if(n[w]=!0,y===!1){for(var p,d=u.mirror||document.createElement("div"),h=d.childNodes,b=h.length,v=0;v<b;++v)if(h[v]._R===n){p=h[v];break}p||(p=d.appendChild(document.createElement("div"))),t=DOMTokenList.call(p,n,i)}else t=new DOMTokenList(n,i);return o(n,e,function(){return t}),delete n[w],t},!0)};u(r.Element,"classList","className"),u(r.HTMLElement,"classList","className"),u(r.HTMLLinkElement,"relList","rel"),u(r.HTMLAnchorElement,"relList","rel"),u(r.HTMLAreaElement,"relList","rel")}(this)}.call(typeof window=="object"&&window||typeof self=="object"&&self||typeof global=="object"&&global||{})});E.Element&&function(m){m.matches=m.matches||m.matchesSelector||m.webkitMatchesSelector||m.msMatchesSelector||function(l){for(var r=this,y=(r.parentNode||r.document).querySelectorAll(l),o=-1;y[++o]&&y[o]!=r;);return!!y[o]}}(Element.prototype);E.Element&&function(m){m.closest=m.closest||function(l){for(var r=this;r.matches&&!r.matches(l);)r=r.parentNode;return r.matches?r:null}}(Element.prototype);Array.prototype.indexOf||(Array.prototype.indexOf=function(m,l){for(var r=l||0,y=this.length;r<y;r++)if(this[r]===m)return r;return-1})});O();})();
//# sourceMappingURL=step-by-step-polyfills.js.map