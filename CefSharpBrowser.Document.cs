﻿//********************************************************************************************
//Developed: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using System.Windows;
using System.Windows.Threading;

namespace Cliver.CefSharpController
{
    public partial class CefSharpBrowser
    {
        public object ExecuteJavaScript(string script)
        {
            var t = browser.EvaluateScriptAsync(
@"(function(){
            try{
            " + script + @"
            }catch(err){
                alert(err.message);
            }
        }())");
            while (!t.IsCompleted)
                DoEvents();
            return t.Result.Result;
        }

        public void HighlightElements(string xpath)
        {
            ExecuteJavaScript(@"
                if(!document.__highlightedElements){
                    var style = document.createElement('style');
                    style.type = 'text/css';
                    style.innerHTML = '.__highlight { background-color: #F00 !important; }';
                    document.getElementsByTagName('head')[0].appendChild(style);
                }else{
                    for(var i = 0; i < document.__highlightedElements.length; i++)
                        document.__highlightedElements[i].className = document.__highlightedElements[i].className.replace(/\b__highlight\b/,''); 
                }

                document.__highlightedElements = [];
                var es = document.__getElementsByXPath('" + xpath + @"');
                for(var i = 0; i < es.length; i++){
                    es[i].className += ' __highlight';
                    document.__highlightedElements.push(es[i]);
                }
            ");
        }

        public void HighlightElementsOnHover()
        {
            ExecuteJavaScript(@"
if(document.__highlightElementsOnHover)
    return;

//var style = document.createElement('style');
//style.type = 'text/css';
////style.innerHTML = '*:hover { background-color: #F00 !important; }';
//style.innerHTML = ':hover { outline: dotted #F00 !important; }';
//document.getElementsByTagName('head')[0].appendChild(style);
//document.__highlightElementsOnHover = style;

var es = document.body.getElementsByTagName('*');
for(var i = 0; i < es.length; i++){
    es[i].addEventListener('mouseover', function( event ) {
        if(document.__highlightedElementOnHover){
            //document.__highlightedElementOnHover.style.outline = '';
            document.__highlightedElementOnHover.style.backgroundColor = '';
        }   
        event.target.style.backgroundColor = '#f70';
        //event.target.style.outline = 'dotted';
        //event.target.style.backgroundColor = 'invert';
        document.__highlightedElementOnHover = event.target;
    }, false);
    //es[i].addEventListener('mouseleave', function( event ) { 
    //    event.target.style.outline = '';
    //}, false);
}
            ");
        }

        public static string Define__getElementsByXPath()
        {
            return @"
if(!document.__getElementsByXPath){
            document.__getElementsByXPath = function(path) {
                var evaluator = new XPathEvaluator();
                var result = evaluator.evaluate(path, document.documentElement, null, XPathResult.UNORDERED_NODE_ITERATOR_TYPE, null);
                var es = [];
                for(var thisNode = result.iterateNext(); thisNode; thisNode = result.iterateNext()){
                    es.push(thisNode);
                }
                return es;
            };
};
            ";
        }

        public static string Define__createXPathForElement()
        {
            return @"
if(!document.__createXPathForElement){
            document.__createXPathForElement = function(element) {
                var xpath = '';
                for (; element && element.nodeType == 1; element = element.parentNode) {
                    //alert(element);
                    var cs = element.parentNode.children;
                    var j = 0;
                    var k = 0;
                    for(var i = 0; i < cs.length; i++){
                        if (cs[i].tagName == element.tagName){
                            j++;
                            if(cs[i] == element){
                                k = j;
                                //break;
                            }
                        } 
                    }
                    var id = '';
                    if(j > 1)
                        id = '[' + k + ']';
                    xpath = '/' + element.tagName.toLowerCase() + id + xpath;
                }
                return xpath;
            };
};
            ";
        }

        public void SetAttribute(string xpath, string attribute, string value)
        {
            ExecuteJavaScript(
                CefSharpBrowser.Define__getElementsByXPath() + @"
var es =  document.__getElementsByXPath('" + xpath + @"');
//if(es.length > 1)
//    alert('Found more than 1 element!');
//if(es.length < 1)
//    alert('no element found:' + '" + xpath + @"');
for(var i = 0; i < es.length; i++){
    var e = es[i];
    //while(e && e.tagName != 'INPUT')
    //    e = e.parentNode;
    //if(e)
        e['" + attribute + @"'] = '" + value + @"';
}
            ");
        }

        public void SetValue(string xpath, string value)
        {
            ExecuteJavaScript(
                CefSharpBrowser.Define__getElementsByXPath() + @"
var es =  document.__getElementsByXPath('" + xpath + @"');
//if(es.length > 1)
//    alert('Found more than 1 element!');
//if(es.length < 1)
//    alert('no element found:' + '" + xpath + @"');
for(var i = 0; i < es.length; i++){
    var e = es[i];
    //while(e && e.tagName != 'INPUT')
    //    e = e.parentNode;
    //if(e)
        e.value = '" + value + @"';
}
            ");
        }

        public List<string> GetAttribute(string xpath, string attribute)
        {
            var os = (List<object>)ExecuteJavaScript(
                 CefSharpBrowser.Define__getElementsByXPath() + @"
var es =  document.__getElementsByXPath('" + xpath + @"');
//if(es.length > 1)
//    alert('Found more than 1 element!');
//if(es.length < 1)
//    alert('no element found:' + '" + xpath + @"');
var vs = [];
for(var i = 0; i < es.length; i++){
    vs.push(es[i].getAttribute('" + attribute + @"'));
}
return vs;
            ");
            List<string> vs = new List<string>();
            foreach (string s in os)
                vs.Add(s);
            return vs;
        }

        public List<string> GetValue(string xpath)
        {
            var os = (List<object>)ExecuteJavaScript(
                 CefSharpBrowser.Define__getElementsByXPath() + @"
var es =  document.__getElementsByXPath('" + xpath + @"');
//if(es.length > 1)
//    alert('Found more than 1 element!');
//if(es.length < 1)
//    alert('no element found:' + '" + xpath + @"');
var vs = [];
for(var i = 0; i < es.length; i++){
    vs.push(es[i].value);
}
return vs;
            ");
            List<string> vs = new List<string>();
            foreach (string s in os)
                vs.Add(s);
            return vs;
        }

        public void Click(string xpath)
        {
            ExecuteJavaScript(
                CefSharpBrowser.Define__getElementsByXPath() + @"
var es =  document.__getElementsByXPath('" + xpath + @"');
if(es.length < 1)
    alert('no element found:' + '" + xpath + @"');
for(var i = 0; i < es.length; i++){
    es[i].click();
}
            ");
        }
    }
}
