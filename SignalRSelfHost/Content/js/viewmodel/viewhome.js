define(['knockout'],
    function (ko) {

        // Viewmodel for home view
        return {
            items: ko.observableArray([
                { id: 1, name: "one" },
                { id: 2, name: "two" },
                { id: 3, name: "three" }
            ])
        };
    });