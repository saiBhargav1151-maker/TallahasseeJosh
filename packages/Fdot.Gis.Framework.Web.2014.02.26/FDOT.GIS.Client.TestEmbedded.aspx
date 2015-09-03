<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FDOT.GIS.Client.TestEmbedded.aspx.cs"
    Inherits="FDOT.GIS.Web.TestEmbedded" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <script type="text/javascript" src="Scripts/all.js"></script>
    <title>FDOT GIS Client Embedded Test</title>
    <style type="text/css">
        html, body
        {
            height: 100%;
            overflow: auto;
        }
        
        body
        {
            padding: 0;
            margin: 0;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 100%;
        }
        
        #container
        {
            margin: 0 auto 0 auto;
        }
        
        #silverlightControlHost
        {
            float: left;
        }
        
        #testOptionsContainer
        {
            height: 394px;
            width: 45%;
            float: left;
            border: solid 1px #aaa;
            font-size: 0.8em;
            padding: 2px;
        }
        
        #toolbar
        {
            clear: both;
            background-color: #dedede;
            border-bottom: solid 1px #aaa;
            padding: 5px;
        }
        
        #toolbar ul
        {
            margin-left: 0;
            padding-left: 0;
            display: inline;
            border: none;
        }
        
        #toolbar ul li
        {
            margin-left: 0;
            padding-left: 2px;
            border: none;
            list-style: none;
            display: inline;
        }
        
        .testDataLayer
        {
            display: none;
        }
        
        .testDataText
        {
            width: 95%;
            height: 275px;
        }
        
        
        table
        {
            background-color: #337;
            font-size: 0.7em;
            border: solid 1px #444;
            -moz-border-radius: 10px;
            -webkit-border-radius: 10px;
            border-radius: 10px;
            margin: 10px;
        }
        
        table tr:last-child td:first-child
        {
            -moz-border-radius-bottomleft: 10px;
            -webkit-border-bottom-left-radius: 10px;
            border-bottom-left-radius: 10px;
        }
        table tr:last-child td:last-child
        {
            -moz-border-radius-bottomright: 10px;
            -webkit-border-bottom-right-radius: 10px;
            border-bottom-right-radius: 10px;
        }
        
        table thead
        {
            color: #FFF;
        }
        
        table tbody
        {
            background-color: #FFF;
        }
        
        table th
        {
            padding: 5px 10px;
        }
        
        table td
        {
            padding: 1px 7px;
        }
        
        table tr.odd
        {
            background-color: #DDD;
        }
        
        table caption
        {
            text-align: left;
            font-size: 1.2em;
            font-weight: bold;
            padding-left: 10px;
        }
    </style>
</head>
<body>
    <div id="container">
        <div id="silverlightControlHost">
        </div>
        <div id="testOptionsContainer">
            <div id="toolbar">
                <button id="draw">
                    Draw</button>
                <select id="selectTestData">
                    <option value=""></option>
                    <option value="drawPointTestData">Point/Symbol</option>
                    <option value="drawLineTestData">Line</option>
                    <option value="drawBoxTestData">Box</option>
                    <option value="drawCircleTestData">Circle</option>
                    <option value="drawPolygonTestData">Polygon</option>
                    <option value="drawPolyLineTestData">PolyLine</option>
                    <option value="drawMultipleTestData">Multiple Items</option>
                </select>
                <ul>
                    <li>
                        <button id="drawClear">
                            Draw Clear</button></li>
                    <li>
                        <button id="zoomToCounty">
                            Zoom To</button></li>
                    <!--<li><button id="loadCodes">Load Codes</button></li>-->
                    <li>
                        <button id="testQuery">
                            Show Query</button></li>
                    <li>
                        <button id="setColor">
                            Set Color</button></li>
                    <li>
                        <button id="DrawFeatures">
                            Draw Features</button></li>
                </ul>
            </div>
            <div>
                <div id="drawTestData">
                    <div id="drawPointTestData" class="testDataLayer">
                        <!--
Values for symbolType are as follows:
        Circle,
        Square,
        Triangle,
        Pentagon,
        Hexagon,
        Octagon,
        Cross,
        Strobe,
        Rotating

Colors are ARGB hex values.
                            -->
                        <textarea id="drawPointTestDataText" class="testDataText" cols="20">{
    "symbolType":"rotating",
    "symbolFillColor":"#FF000000",
    "symbolStrokeColor":"#FFFFFFFF",
    "DrawingObjects":
    [
        {
            "x":-9382081.26811,
            "y":3558593.08359
        }
    ]
}</textarea>
                    </div>
                    <div id="drawLineTestData" class="testDataLayer">
                        <textarea id="drawLineTestDataText" class="testDataText" cols="20">{
    "DrawingObjects":
    [
        {
            "paths":[
                [
                    [-9480355.60194072,3286421.3568361937],
                    [-8820697.7989578787,3280660.1533166929]
                ]
            ]
        }
    ]
}</textarea>
                    </div>
                    <div id="drawBoxTestData" class="testDataLayer">
                        <textarea id="drawBoxTestDataText" class="testDataText" cols="20">{
    "DrawingObjects":
    [
        {
            "xmin":-9383855.4429890811,
            "xmax":-9052586.2406177856,
            "ymin":3172637.5873260531,
            "ymax":3459257.462421217
        }
    ]
}</textarea>
                    </div>
                    <div id="drawCircleTestData" class="testDataLayer">
                        <textarea id="drawCircleTestDataText" class="testDataText" cols="20">{
    "DrawingObjects":
    [
        {
            "x":-9391697.881,
            "y":2949365.74568,
            "radius":129437.9561
        }
    ]
}</textarea>
                    </div>
                    <div id="drawPolygonTestData" class="testDataLayer">
                        <textarea id="drawPolygonTestDataText" class="testDataText" cols="20">{
    "DrawingObjects":
    [
        {
            "rings":[
                [
                    [-9510601.9204180986,3067495.6230951641],
                    [-9235504.4523619358,2904741.6236692667],
                    [-9117399.78021217,3057413.5169360377],
                    [-9316161.3016349468,3133749.4635694232],
                    [-9582616.9644118585,3158234.5785273016],
                    [-9510601.9204180986,3067495.6230951641]
                ]
            ]
        }
    ]
}</textarea>
                    </div>
                    <div id="drawPolyLineTestData" class="testDataLayer">
                        <textarea id="drawPolyLineTestDataText" class="testDataText" cols="20">{
    "DrawingObjects":
    [
        {
            "paths":[
                [
                    [-8706914.0294477381,3460697.7633010922],
                    [-8984892.0992636513,3060294.1186957881],
                    [-8725637.9408861175,2943629.747425897],
                    [-8823578.40071763,3204324.2066833074],
                    [-8532637.622982841,3140950.9679687992],
                    [-8660824.4012917317,3272018.3480374417],
                    [-8660824.4012917317,3272018.3480374417]
                ]
            ]
        }
    ]
}</textarea>
                    </div>
                    <div id="drawMultipleTestData" class="testDataLayer">
                        <textarea id="drawMultipleTestDataText" class="testDataText" cols="20">{
"doclearfirst":true,
"dopan":true,
"dozoom":false,
"DrawingObjects":
[
    {
        "xmin":-9383855.4429890811,
        "xmax":-9052586.2406177856,
        "ymin":3172637.5873260531,
        "ymax":3459257.462421217
    },
    {
        "paths":[
            [
                [-9480355.60194072,3286421.3568361937],
                [-8820697.7989578787,3280660.1533166929]
            ]
        ]
    },
    {
        "x":-9391697.881,
        "y":2949365.74568,
        "radius":129437.9561
    }
]
}</textarea>
                    </div>
                    <div id="codesTestData" class="testDataLayer">
                        <select id="codeLists">
                            <option value=""></option>
                        </select>
                        <select id="codesList">
                            <option value=""></option>
                        </select>
                    </div>
                    <div id="queryTestData" class="testDataLayer">
                        <select id="queryableLayers">
                            <option value=""></option>
                        </select>
                        <select id="layerFields">
                            <option value=""></option>
                        </select><br />
                        <button id="executeQuery">
                            Execute</button>
                        <textarea id="queryTestDataText" class="testDataText" cols="20" style="height: 225px;">{
    "ClientData":
    {
        "Key":"FOO",
        "Title": "Test query",
        "Id":1,
        "Caption":"This is the table's caption."
    },
    "Options":
    {
        "FeatureType":{"Id":"gev-dynamic|1"},
        "QueryCriteria":
        {
            "Attribute": { "Name":"CONTYDOT" },
            "Value":"26",
            "ComparisonType":0
        },
        "SecondaryCriteria":
        [
            {   
                "Attribute": { "Name":"LGHTCOND" },
                "Value":"01",
                "ComparisonType":0,
                "LogicalOperator":0
            },
            {
                "Attribute": { "Name":"WEATCOND" },
                "Value":"01",
                "ComparisonType":0,
                "LogicalOperator":0
            }
        ],
        "GeometryCriteria":
        {
            "Geometry":
                    {
                        "Xmin":-9191487.6328293085,
                        "Xmax":-9146480.03070931,
                        "Ymin":3423668.6900953464,
                        "Ymax":3484832.8673353465
                    }
        }
    }
}</textarea>
                        NOTE: ReturnAllFields set to true will return all layer fields, whether or not specific
                        fields are specified in ReturnFields.
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div style="clear: both;">
        <table id="queryResults" cellspacing="0">
            <caption>
                caption</caption>
            <thead>
                <th>
                    column 1
                </th>
                <th>
                    column 2
                </th>
                <th>
                    column 3
                </th>
            </thead>
            <tbody>
                <tr>
                    <td>
                        R1:C1
                    </td>
                    <td>
                        R1:C2
                    </td>
                    <td>
                        R1:C3
                    </td>
                </tr>
                <tr class="odd">
                    <td>
                        R2:C1
                    </td>
                    <td>
                        R2:C2
                    </td>
                    <td>
                        R2:C3
                    </td>
                </tr>
                <tr>
                    <td>
                        R3:C1
                    </td>
                    <td>
                        R3:C2
                    </td>
                    <td>
                        R3:C3
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
    <fieldset style='width: 400px;'>
        <legend>Layer List</legend>
        <div id='toc-container' style='clear: both; height: 200px; overflow: scroll;'>
        </div>
    </fieldset>
    <script type="text/javascript">
        function buildToc() {
            FDOT.Map.executeCommand('listlayers', null,
                    function (r) {
                        $('#toc-container').children().remove();
                        $.each(r.Result, function (index, item) {
                            var i = item;
                            var p = $('<p />');
                            var checkboxid = 'toggle-' + i.MapServiceName + '|' + i.LayerId;
                            var checkbox = $('<input type="checkbox" />');
                            checkbox.attr('id', checkboxid);
                            checkbox.click(function () {
                                FDOT.Map.executeCommand('togglelayer', i, function (res) {
                                    console.log('toggled layer: ' + i.Name + ':' + res.Result);
                                    if (res.Result) {
                                        checkbox.attr('checked', 'checked');
                                    }
                                    else {
                                        checkbox.removeAttr('checked');
                                    }
                                });

                            });
                            var label = $('<label for="' + checkboxid + '" />');
                            label.text(i.Name);

                            p.append(checkbox);
                            p.append(label);
                            $('#toc-container').append(p);
                        });
                    }
                );
        }

        $(function () {

            var urlParams = {};
            (function () {
                var e,
                    a = /\+/g,  // Regex for replacing addition symbol with a space
                    r = /([^&=]+)=?([^&]*)/g,
                    d = function (s) { return decodeURIComponent(s.replace(a, " ")); },
                    q = window.location.search.substring(1);

                while (e = r.exec(q))
                    urlParams[d(e[1])] = d(e[2]);
            })();

            FDOT.Map.create
                ({
                    // need to override the defaults set in Map.js 
                    // when in standard mode, all are set to true there except showOverviewMap
                    // when in embedded mode, all are set to false
                    embedded: true,
                    showScalebar: true,
                    showNavigation: true,
                    containerSelector: '#silverlightControlHost',
                    id: 'slctl',
                    height: '400px',
                    width: '500px',
                    appId: urlParams.appId,
                    onReady: function () {
                        //FDOT.Map.addQueryCompletedHandler(onQueryCompleted);
                        FDOT.Map.addLayerLoadedHandler(onLayerLoaded);
                        //                        FDOT.Map.addExtentChangeHandler(onExtentChange);
                        //                        FDOT.Map.addVisibleLayersChangeHandler(onLayersChange);

                        buildToc();

                    }
                });

            $("#queryResults tr").live('click', rowClicked);
            $("#queryResults tr").live('mouseenter', mouseEnter);
            $("#queryResults tr").live('mouseleave', mouseLeave);

            // UI EVENT HANDLERS
            $('#zoomToCounty').click(function () {
                FDOT.Map.executeCommand('zc', { value: 'Leon', highlight: true, buffer: 1.1 }, function () { });

                // work program test/example
                //FDOT.Map.executeCommand('zwp', { value: '4252801', highlight: 'true', buffer: 2.0 }, function () {});

            });

            $('#draw').click(function () {
                FDOT.Map.executeCommand('draw', jQuery.parseJSON(getVisibileData()));
            });


            $('#DrawFeatures').click(function () {
                FDOT.Map.executeCommand('drawFeature',
                        {
                            SymbolUrl: 'http://tlbstws3.dot.state.fl.us/gisframework/Images/Icons/Weather/Hurricane.png',
                            NameKey: '1',
                            DrawFeatures: [
                            {
                                Geometry: { x: -9510601.92, y: 3067495.62 },
                                Attributes: { "1": "1", "2": "2" }
                            },
                                                        {
                                                            Geometry: { x: -9510601.92, y: 3267495.62 },
                                                            Attributes: { "1": "http://www.google.com", "2": "3" }
                                                        }, {
                                                            Geometry: { x: -9510601.92, y: 3167495.62 },
                                                            Attributes: { "1": "3", "2": "http://www.google.com/||GOOGLE" }
                                                        }
                        ]
                        }
                  );
            });

            $('#drawClear').click(function () {
                FDOT.Map.executeCommand('draw', { "doclearfirst": true });
            });

            $('#selectTestData').change(function () {
                $('.testDataLayer').hide();     // hide the other test data fields
                $('#selectTestData option:selected').each(function () {
                    $('#' + $(this).val()).show();
                });
            });

            $('#loadCodes').click(function () {
                FDOT.Map.executeCommand("codes", codesToFetch, onCodesLoaded);
            });

            $('#codeLists').change(function () {
                $('#codeLists option:selected').each(function () {
                    loadCodeList($(this).val());
                });
            });

            $('#testQuery').click(function () {
                showTestLayer($('#queryTestData'));
                loadQueryableLayers();
            });

            $('#executeQuery').click(function () {
                FDOT.Map.executeCommand('query', jQuery.parseJSON(getVisibileData()), onQueryCompleted);
            });

            $('#queryableLayers').change(function () {
                $('#queryableLayers option:selected').each(function () {
                    FDOT.Map.loadLayer({ "LayerId": $(this).val() });
                });
            });

            $('#setColor').click(function () {
                FDOT.Map.setPalette('#957F9F', 0.0);
            });
            // END UI EVENT HANDLERS

        });

        // BEGIN -- CODE LIST SUPPORT

        // Codes can be pre-loaded if you know what lists you need in advance - the code manager in Silverlight
        // caches them, so doing a load command early on could enhance perceived speed for a user.
        // The event handler for the loadCodes button (see above) is preloading using codesToFetch.
        var codesToFetch = ["county", "lghtcond", "rdsurfcd"];
        var codeCollections = null;

        // Cache the codes returned and load the list of code collections returned.
        // the args (a) contains a Keys property, containing the names of all the code collections it contains
        // and a Codes property, which is a keyed dictionary containing all the code sets returned.
        function onCodesLoaded(s, a) {
            $.extend(codeCollections, a);
            showTestLayer($('#codesTestData'));
            populateSelect($('#codeLists'), codeCollections.Keys);
        }

        // load the codes for the selected code collection
        function loadCodeList(collectionName) {
            if (collectionName.length > 0) {
                populateSelect($('#codesList'), codeCollections.Codes[collectionName], 'Value', 'Description');
            }
        }
        // END -- CODE LIST SUPPORT


        // BEGIN -- QUERY SUPPORT
        function onQueryCompleted(a) {

            //var clientdata = a.ClientData;
            var queryResult = a.Result;

            console.log("QUERY RESULTS RETURNED [" + queryResult.length + " ROWS]");

            var table = $("#queryResults");
            table.find("thead").remove();    // clear the table header
            table.find("tbody").remove();    // clear the table body
            table.find("caption").remove();  // clear the table caption

            var header = "<thead><tr>";
            //header += "<th>Zoom</th>";
            $.each(queryResult.FeatureType.Attributes, function (i, key) {
                header += "<th>" + key.Description + "</th>";
            });
            header += "</tr></thead>";
            table.append(header);

            //                var clientData = JSON.parse(clientdata);
            //                console.log(clientData);
            //                var caption = "<caption>" + clientData["Caption"] + "</caption>";
            //                console.log(caption);
            //                table.append(caption);

            // start writing the results to the table body
            var body = $("<tbody />");
            table.append(body);            // append the tbody closing tag

            // calling the inner function like this allows the results to be rendered w/o hanging up the browser - it's more
            // or less an asynchronous write for the table.  We're waiting until the HTML has been consructed for all rows before
            // rendering them, as rendering partial results sometimes ends up with things being out of order.
            var currentRow = 0;
            var geometries = new Array();

            (function () {
                var endIndex = currentRow + 15;
                for (currentRow; currentRow < (Math.min(endIndex, queryResult.Features.length)); currentRow++) {
                    var row = queryResult.Features[currentRow];

                    var tableRow = $('<tr />');

                    //                    var rownumcell = $('<td />');
                    //                    var innerHtml = '<img src="magnifier.png" alt="Zoom To Parcel" title="Zoom To Parcel" name="ZoomToParcel" style="cursor:pointer"';
                    //                    innerHtml += ' id="' + row.DisplayValues["EXCS_PRCL_SQ"] + '"/>';
                    //                    rownumcell.html(innerHtml);

                    //tableRow.append(rownumcell);
                    $.each(row.DisplayValues, function (j, k) {
                        var valueCell = $('<td />');
                        valueCell.text(k);
                        tableRow.append(valueCell);
                    });
                    body.append(tableRow);

                    tableRow.data('objectId', row.Attributes[row.DisplayName]);
                    tableRow.data('geometry', row.Geometry);

                    geometries.push(row.Geometry);
                }

                // After a set of rows have been rendered, let the browser catch up and then call for another set
                // until the results have all been rendered.  A loading graphic would be a good addition, then swap it
                // out once the results are ready to be rendered.  A jQuery table plugin would be nice as well, to apply
                // some better styling and features here.
                if (currentRow < queryResult.length) {
                    setTimeout(arguments.callee, 50);
                }

            })();
            $('#queryResults').show();
            //$("#queryResults").tablesorter({ widgets: ['zebra'] });

            //            visibleGeometries = geometries;
            //            if (graphicsManagerId == null) {
            //                FDOT.Map.executeCommand("draw", { "DrawingObjects": geometries, "UseNewGraphicsManager": true }, function (result) { graphicsManagerId = result.Result; });
            //            }
            //            else {
            //                FDOT.Map.executeCommand("draw", { "DrawingObjects": geometries, "DoClearFirst": true, "GraphicsManagerId": graphicsManagerId });
            //            }
            //            $('img[name=ZoomToParcel]').click(function () {
            //                var zoomGeometry = $(this).closest('tr').data('geometry');
            //                FDOT.Map.executeCommand("draw", { "DrawingObjects": [zoomGeometry], "DoZoom": true, "Buffer": 1.5 });
            //            });

            // After a set of rows have been rendered, let the browser catch up and then call for another set
            // until the results have all been rendered.  A loading graphic would be a good addition, then swap it
            // out once the results are ready to be rendered.  A jQuery table plugin would be nice as well, to apply
            // some better styling and features here.
            //                        if (currentRow < queryResult.length) {
            //                            setTimeout(arguments.callee, 50);
            //                        }

            //                    })();
            // } catch (e) {console.log(e); }

        }

        // row-click event handler
        function rowClicked(s, a) {
            alert($(this).data("objectId"));
            // while we're not doing anything useful in here, if we had attached the result's geometry as data,
            // that could be passed off to the draw command to locate the result on the map.  It could also serve
            // as a jumping off point for selecting rows to export, etc.
        }

        function mouseEnter(s, a) {
            var geometry = $(this).data("geometry");
            if (geometry === null || geometry === undefined || geometry === '') return;

            FDOT.Map.executeCommand("draw", { "doclearfirst": true, "DrawingObjects": [geometry] });
        }

        function mouseLeave(s, a) {
            FDOT.Map.executeCommand("draw", { "doclearfirst": true });
        }

        // loads the queryable layers
        function loadQueryableLayers() {
            var queryablelayers = FDOT.Map.getQueryableLayers();
            populateSelect($('#queryableLayers'), queryablelayers, 'Id', 'Name');
        }

        // loads the visible layers
        function loadVisibleLayers() {
            var visiblelayers = FDOT.Map.getVisibleLayers();
            // do something with the list here
            // returns LayerIdentifier, which has properties MapServiceName (string), LayerId (int)
        }

        // loads the fields applicable for the selected layer
        function onLayerLoaded(s, a) {
            populateSelect($('#layerFields'), a.Layer.Fields, 'Name', 'Alias');
        }
        // END -- QUERY SUPPORT


        // BEGIN -- GENERAL UTILITY FUNCTIONS
        function populateSelect(list, collection, valueProperty, labelProperty) {
            $('option', list).remove();
            list.append($('<option></option>').val('').html(''));

            // jQuery.each always throws an exception on the last element of the returned collections
            // it's a .NET exception, so possibly something to do with not getting/handling the count correctly
            try {
                $.each(collection, function (i, obj) {
                    var value = valueProperty != null ? (valueProperty.length > 0 ? obj[valueProperty] : obj) : obj;
                    var label = labelProperty != null ? (labelProperty.length > 0 ? obj[labelProperty] : obj) : obj;
                    list.append($('<option></option>').val(value).html(label));
                });
            } catch (e) { }
        }


        // show a given test layer
        function showTestLayer(layer) {
            $('.testDataLayer').hide();
            layer.show();
        }

        // this is just for the test data layer visibility toggling
        function getVisibileData() {
            var result = '';
            $('.testDataLayer').each(function () {
                if ($(this).css("display") != 'none')
                { result = $(this).find('textarea').val(); }
            });
            return result;
        }
        // END -- GENERAL UTILITY FUNCTIONS

        // BEGIN -- LOGGING console workaround for IE and any case where console is unavailable
        if (!window.console) console = {};
        console.log = console.log || function () { };
        console.warn = console.warn || function () { };
        console.error = console.error || function () { };
        console.info = console.info || function () { };
        // END -- LOGGING
    </script>
</body>
</html>
<%--This is what a query result row looks like - the data fields depend on the layer
[
    {
        "Data":
        {
            "CARNUM":"710909630",
            "OBJECTID":"27640",
            "RDWYID":"26050000",
            "CALYEAR":"2009",
            "CRASHDTE":"2/21/2009 12:00:00 AM",
            "CONTYDOT":"26",
            "ROUTEID":"SR   24 ",
            "USRTNO":"        ",
            "DHSPOPCD":" ",
            "RDSURFCD":"01",
            "WEATCOND":"01",
            "LGHTCOND":"01",
            "ACCISEV":"1",
            "ALCINVCD":"0",
            "FATALINV":"2",
            "DHSINJCD":"1",
            "LOCNODE":"00970",
            "LOCMP":"16.442",
            "TOT_OF_FATL_NUM":"0",
            "TOT_OF_INJR_NUM":"0",
            "TOT_OF_PEDST_NUM":"0",
            "TOTOF_PEDLCYCL_NUM":"0",
            "SKTRESNM":"44",
            "FRST_RDCND_CRSH_CD":"01",
            "SCND_RDCND_CRSH_CD":"00",
            "SITELOCA":"03",
            "CRRATECD":"40",
            "ACCSIDRD":"R",
            "ACCLANE":"U",
            "TOTSEVREINJ_NUM":"0"
        },
        "Geometry":
        {
            "spatialReference":null,
            "x":-9146838.3533,
            "y":3476650.5151,
            "points":null,
            "paths":null,
            "rings":null,
            "xmin":null,
            "xmax":null,
            "ymin":null,
            "ymax":null
        }
    }
]--%>
