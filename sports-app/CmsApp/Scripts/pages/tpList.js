(function () {
    'use strict';
    var lib = {};
    lib.playerUpdateFailure = function (result) {
        alert("Failed to update player: \n\n" + (result.responseJSON.statusText || JSON.stringify(result)));
        //console.log("result: ", result);
        if ((typeof result.responseJSON.Id === "number") && (typeof result.responseJSON.oldShirtNum === "number")) {
            var inp = $("#shirtNum_" + result.responseJSON.Id);
            //console.log("input: ", inp);
            $(inp).val(result.responseJSON.oldShirtNum);
        }
    };
    window.tpList = lib;
})();
//# sourceMappingURL=tpList.js.map