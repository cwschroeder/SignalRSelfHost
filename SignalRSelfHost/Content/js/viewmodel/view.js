define(function () {
    // Basic view
    return function view(title, templateName, model) {
        var self = this;

        self.title = title;
        self.templateName = templateName;
        self.model = model; // the view's knockout viewmodel
    };
});