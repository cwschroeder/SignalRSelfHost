define(['knockout'],
    function (ko) {
        // Basic view
        return function basicView(title, templateName, modelName) {
            var self = this;

            self.title = ko.observable(title);
            self.templateName = ko.observable(templateName);
            self.modelName = ko.observable(modelName);

            self.template = ko.observable();
            self.model = ko.observable();

            self.needsModelInit = ko.observable(true);
        };
    });