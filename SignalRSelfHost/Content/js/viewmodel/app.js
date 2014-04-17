define(['jquery', 'knockout', 'viewmodel/basicview'],
    function (jquery, ko, basicview) {

        // Basic application view model
        return function appViewModel() {

            var self = this;

            // Array containing all viewmodels for navigation
            self.views = ko.observableArray([
                new basicview("home", "home.html", "viewhome"),
                new basicview("energylive", "energylive.html", "viewenergylive")
            ]);

            // this is the data-bound item of the main template
            self.currentView = ko.observable();

            // on_view_changed event handler
            self.currentView.subscribe(function (newView) {
                console.log("View changed to: " + newView.title());
            });

            // called by Sammy on observed #-link clicks
            self.navigateToView = function (viewTitle) {
                var match = ko.utils.arrayFirst(self.views(), function (item) {
                    return item.title() == viewTitle;
                });
                if (match) {
                    // set and initialize detail view 
                    require(['text!../../../Views/' + match.templateName(), 'viewmodel/' + match.modelName()],
                         function (template, model) {
                             match.template(template);
                             match.model(new model());

                             //console.log("template: " + match.template() + " !");
                             jquery('#tpl').empty();
                             jquery('#tpl').append(match.template());

                             // init model if needed
                             if (typeof(match.model().init) === "function" && match.needsModelInit() == true) {
                                 match.model().init();
                                 match.needsModelInit(false);
                             }
                             
                             self.currentView(match);
                         });
                } else {
                    console.log("Unknown view: " + viewTitle);
                }
            };

            // init with home view
            //self.navigateToView(self.views()[0]);
        }
    })
