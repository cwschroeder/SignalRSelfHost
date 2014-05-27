define(['knockout'],
    function (ko) {

        // Viewmodel for energy live view
        return function() {
            var self = this;

            self.firstname = ko.observable("Bobby");
            self.lastname = ko.observable("Smitty");
            self.email = ko.observable("bob@smi.de");
            self.password = ko.observable("psasdfasdfasdf");

            self.init = function() {
                //alert("energy live init()");
                //jquery('#tpl').append(templateEnergyLive);
            }
        }
    });