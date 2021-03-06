﻿/* Copyright © 2019 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

angular.module("umbraco").controller("Umbraco.Community.Contentment.DataEditors.RenderMacro.Controller", [
    "$scope",
    "$routeParams",
    "macroResource",
    function ($scope, $routeParams, macroResource) {

        // console.log("render-macro.model", $scope.model);

        var vm = this;

        function init() {

            vm.loading = true;

            if (_.isEmpty($scope.model.config.macro) === false) {

                var macro = _.first($scope.model.config.macro);

                angular.extend(macro.params, {
                    "__propertyAlias": $scope.model.alias,
                    "__propertyLabel": $scope.model.label,
                    "__propertyCulture": $scope.model.culture,
                    "__propertyDataTypeKey": $scope.model.dataTypeKey,
                    "__propertyDescription": $scope.model.description,
                    "__propertyHideLabel": $scope.model.hideLabel,
                });

                macroResource.getMacroResultAsHtmlForEditor(macro.alias, $routeParams.id, macro.params).then(
                    function (result) {
                        vm.html = result;
                        vm.loading = false;
                    },
                    function (error) {
                        vm.error = {
                            title: error.data.Message + " " + error.errorMsg,
                            message: error.data.ExceptionMessage
                        };
                        vm.loading = false;
                    }
                );
            } else {

                vm.error = {
                    title: "Macro not configured",
                    message: "This data type has not been configured. Please ensure that a macro has been selected."
                };

                vm.loading = false;

            }
        };

        init();
    }
]);
