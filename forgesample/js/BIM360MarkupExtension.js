/////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved
// Written by Forge Partner Development
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////


// *******************************************
// BIM 360 Markup Extension
// *******************************************
function BIM360MarkupExtension(viewer, options) {
  Autodesk.Viewing.Extension.call(this, viewer, options);
  this.panel = null; // create the panel variable
  this.containerId = null;
  this.markups = null;
}

BIM360MarkupExtension.prototype = Object.create(Autodesk.Viewing.Extension.prototype);
BIM360MarkupExtension.prototype.constructor = BIM360MarkupExtension;

BIM360MarkupExtension.prototype.load = function () {
  if (this.viewer.toolbar) {
    // Toolbar is already available, create the UI
    this.createUI();
  } else {
    // Toolbar hasn't been created yet, wait until we get notification of its creation
    this.onToolbarCreatedBinded = this.onToolbarCreated.bind(this);
    this.viewer.addEventListener(av.TOOLBAR_CREATED_EVENT, this.onToolbarCreatedBinded);
  }
  return true;
};

BIM360MarkupExtension.prototype.onToolbarCreated = function () {
  this.viewer.removeEventListener(av.TOOLBAR_CREATED_EVENT, this.onToolbarCreatedBinded);
  this.onToolbarCreatedBinded = null;
  this.createUI();
};

BIM360MarkupExtension.prototype.createUI = function () {
  var _this = this;

  // prepare to execute the button action
  var bim360MarkupToolbarButton = new Autodesk.Viewing.UI.Button('runBIM360MarkupCode');
  bim360MarkupToolbarButton.onClick = function (e) {
    // check if the panel is created or not
    if (_this.panel == null) {
      _this.panel = new BIM360MarkupPanel(_this.viewer, _this.viewer.container, 'bim360MarkupPanel', 'BIM 360 Markups');
    }
    // show/hide docking panel
    _this.panel.setVisible(!_this.panel.isVisible());

    // if panel is NOT visible, exit the function
    if (!_this.panel.isVisible()) return;
    // ok, it's visible, let's get the summary!

    _this.panel.removeAllProperties();

    _this.loadMarkups();
  };
  // BIM360MarkupToolbarButton CSS class should be defined on your .css file
  // you may include icons, below is a sample class:
  bim360MarkupToolbarButton.addClass('bim360MarkupToolbarButton');
  bim360MarkupToolbarButton.setToolTip('Markups');

  // SubToolbar
  this.subToolbar = (this.viewer.toolbar.getControl("MyAppToolbar") ?
    this.viewer.toolbar.getControl("MyAppToolbar") :
    new Autodesk.Viewing.UI.ControlGroup('MyAppToolbar'));
  this.subToolbar.addControl(bim360MarkupToolbarButton);

  this.viewer.toolbar.addControl(this.subToolbar);
};

BIM360MarkupExtension.prototype.unload = function () {
  this.viewer.toolbar.removeControl(this.subToolbar);
  return true;
};

Autodesk.Viewing.theExtensionManager.registerExtension('BIM360MarkupExtension', BIM360MarkupExtension);


// *******************************************
// BIM 360 Markup Panel
// *******************************************
function BIM360MarkupPanel(viewer, container, id, title, options) {
  this.viewer = viewer;
  Autodesk.Viewing.UI.PropertyPanel.call(this, container, id, title, options);
}
BIM360MarkupPanel.prototype = Object.create(Autodesk.Viewing.UI.PropertyPanel.prototype);
BIM360MarkupPanel.prototype.constructor = BIM360MarkupPanel;

// *******************************************
// jsTree functions, may be replaced on different implementations...
// *******************************************
function getSelectedNode() {
  var node = $('#userHubs').jstree(true).get_selected(true)[0];
  for (var i = 0; i < node.parents.length; i++) {
    var parent = node.parents[i];
    if (parent.indexOf('hubs') > 0 && parent.indexOf('projects') > 0)
      return { 'project': parent, 'urn': (node.type == 'versions' ? id(node.parents[0]) : '') };
  }
  return null;
}

function id(href) {
  return href.substr(href.lastIndexOf('/') + 1, href.length);
}

// *******************************************
// Markup specific features
// *******************************************
BIM360MarkupExtension.prototype.loadMarkups = function (containerId, urn) {
  var _this = this;
  var selected = getSelectedNode();
  _this.getContainerId(selected.project, selected.urn, function () {
    // ready to load markups...
    _this.getMarkups(_this.containerId, selected.urn);
  });
}

BIM360MarkupExtension.prototype.getContainerId = function (href, urn, cb) {
  var _this = this;
  jQuery.ajax({
    url: '/api/forge/bim360/container?href=' + href,
    success: function (res) {
      _this.containerId = res.container.id
      _this.panel.addProperty('Container Id', _this.containerId, 'API Info');
      cb();
    }
  });
}

BIM360MarkupExtension.prototype.getMarkups = function (containerId, urn) {
  var _this = this;
  urn = btoa(urn);
  jQuery.get('/api/forge/bim360/container/' + containerId + '/markups/' + urn, function (data) {
    _this.markups = data;
    var index = 1;
    data.forEach(function (markup) {
      var dateCreated = moment(markup.attributes.created_at);
      _this.panel.addProperty('Author', markup.attributes.tags.userName, 'Markup ' + index);
      _this.panel.addProperty('Created at', dateCreated.format('MMMM Do YYYY, h:mm a'), 'Markup ' + index);
    })
  }).fail(function (error) {
    alert('Cannot read markups');
  });
}