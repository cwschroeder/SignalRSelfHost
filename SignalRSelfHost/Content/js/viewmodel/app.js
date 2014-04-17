define(['knockout', 'viewmodel/view', 'viewmodel/viewhome', 'viewmodel/viewenergylive'],
    function (ko, view, viewHome, viewEnergyLive) {

        // Basic application view model
        return function appViewModel() {
            var self = this;

            self.views = ko.observableArray([
                new view("Home", "ViewHome", viewHome),
                new view("EnergyLive", "ViewEnergyLive", viewEnergyLive)
            ]);

            self.currentView = ko.observable(self.views()[0]);

            self.navigateToView = function (viewTitle) {
                var match = ko.utils.arrayFirst(self.views(), function (item) {
                    return item.title == viewTitle;
                });
                if (match) {
                    self.currentView(match);
                } else {
                    log("Unknown view: " + viewTitle);
                }
            };
        }
    })
