var app = angular.module('app', ['ngCookies']);
app.controller('DefaultController', function ($scope, $http, $cookies) {
    $scope.token = $cookies.get('token');
    $scope.session = $scope.token != null;
    $scope.href = window.document.location.href;
    $scope.profile;
    $scope.posts;
    $scope.Login = function () {
        $http({
            method: 'POST',
            url: 'http://localhost/socialnet/api/Login?username=' + $scope.username + '&password=' + $scope.password,
            //data: { username: $scope.username, password: $scope.password }
        })
            .then(function (res) {
                console.log(res.data);
                if (res.data.success) {
                    $cookies.put('token', res.data.token);
                    window.location.replace("/social/home");
                }
                else
                    $("#errorMessage").text("Wrong credentials");
            });
    }
    $scope.CreatePost = function() {
        $http({
            method: 'POST',
            url: 'http://localhost/socialnet/api/CreatePost',
            data: { Title: $scope.title, Body: $scope.body },
            headers: {
                'authorization': 'bearer ' + $scope.token
            }
        })
            .then(function (res) {
                console.log(res.data);
                if (res.data.success) {
                    //window.location.replace("/social/posts");
                }
                else
                    $("#errorMessage").text("Request couldn't complete");
            });
    }
    $scope.GetPosts = function () {
        $http({
            method: 'GET',
            url: 'http://localhost/socialnet/api/GetPosts',
            headers: {
                'authorization': 'bearer ' + $scope.token
            }
        })
            .then(function (res) {
                console.log(res.data);
                if (res.data.success) {
                    $scope.posts = res.data.posts;
                }
            });
    }
    $scope.Profile = function () {
        $http({
            method: 'GET',
            url: 'http://localhost/socialnet/api/GetProfile',
            headers: {
                'authorization': 'bearer ' + $scope.token
            }
        })
            .then(function (res) {
                if (res.data.success) {
                    $scope.profile = res.data.user;
                }
                else
                    window.location.replace("/social/login");
            });
    }
    $scope.OnLoad = function () {
        //if ($scope.token != null && $scope.href.includes("login"))
        //    window.location.replace("/social/home");
        //if ($scope.token == null && !$scope.href.includes("login"))
        //    window.location.replace("/social/login");
        if ($scope.href.includes("posts"))
            $scope.GetPosts();
        $scope.Profile();
    }
    $scope.OnLoad();
});