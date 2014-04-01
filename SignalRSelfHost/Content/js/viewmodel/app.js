var View = function(title, templateName, data) {
    var self = this;

    self.title = title;
    self.templateName = templateName;
    self.data = data;
};

var viewHome = {
    items: ko.observableArray([
        { id: 1, name: "one" },
        { id: 2, name: "two" },
        { id: 3, name: "three" }
    ])
};

var viewEnergyLive = {
    firstName: ko.observable("Bob"),
    lastName: ko.observable("Smith")
};



function AppViewModel() {
    var self = this;

    self.views = ko.observableArray([
        new View("Home", "ViewHome", viewHome),
        new View("EnergyLive", "ViewEnergyLive", viewEnergyLive)
    ]);

    self.currentView = ko.observable(self.views()[0]);

    self.navigateToView = function (viewTitle) {
        var match = ko.utils.arrayFirst(self.views(), function(item) {
            return item.title == viewTitle;
        });
        if (match) {
            self.currentView(match);
        } else {
            log("Unknown view: " + viewTitle);
        }
    };
}