var app = angular.module('app', ['ngCookies']);
app.controller('DefaultController', function ($scope, $http, $cookies) {
    $scope.token = $cookies.get('token');
    $scope.session = $scope.token != null;
    $scope.href = window.document.location.href;
    $scope.profile;
    $scope.posts;
    $scope.follows;
    $scope.followers;
    $scope.Login = function () {
        $http({
            method: 'POST',
            url: 'http://localhost/socialnet/api/Login?username=' + $scope.username + '&password=' + $scope.password,
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
    $scope.Logout = function () {
        $cookies.remove('token');
        $scope.token = null;
        window.location.replace("/social/login");
    }
    $scope.CreatePost = function () {
        console.log($scope.media);
        $http({
            method: 'POST',
            url: 'http://localhost/socialnet/api/CreatePost',
            data: {
                Title: $scope.title,
                Body: $scope.body,
                Media: $scope.media
            },
            headers: {
                'authorization': 'bearer ' + $scope.token
            }
        })
            .then(function (res) {
                console.log(res.data);
                if (res.data.success) {
                    window.location.replace("/social/posts");
                }
                else
                    $("#errorMessage").text("Request couldn't complete");
            });
    }
    $scope.Follow = function (id) {
        $http({
            method: 'GET',
            url: 'http://localhost/socialnet/api/Follow?id=' + id,
            headers: {
                'authorization': 'bearer ' + $scope.token
            }
        })
            .then(function (res) {
                console.log(res.data);
                if (res.data.message == 'Followed')
                    $('fb' + id).text = 'Unfollow'
                else if (res.data.message == 'Unfollowed')
                    $('fb' + id).text = 'Follow'
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
    $scope.GetProfile = function () {
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
                    $scope.follows = res.data.follows;
                    $scope.followers = res.data.followers;
                    console.log($scope.follows);
                }
                else
                    window.location.replace("/social/login");
            });
    }
    $scope.DeleteEverything = function () {
        $http({
            method: 'GET',
            url: 'http://localhost/socialnet/api/DeleteEverything',
            headers: {
                'authorization': 'bearer ' + $scope.token
            }
        })
            .then(function (res) {
                if (res.data.success) {
                    $("#successMessage").text("Operation succeed");
                }
            })
            .catch(function () {
                $("#errorMessage").text("You have to be authorized to perform this action");
            });
    }
    $scope.Register = function () {
        $http({
            method: 'POST',
            url: 'http://localhost/socialnet/api/Register',
            data: {
                Username: $scope.username,
                Password: $scope.password,
                Name: $scope.name,
                Lastname: $scope.lastname,
                DateBirth: $scope.datebirth,
                Email: $scope.email,
                Role: 'user'
            }
        })
            .then(function (res) {
                console.log(res.data);
                if (res.data.success) {
                    window.location.replace("/social/login");
                }
                else
                    $("#errorMessage").text(res.data.error);
            });
    }
    $scope.GetUsers = function () {
        $http({
            method: 'GET',
            url: 'http://localhost/socialnet/api/GetAllUsers',
            headers: {
                'authorization': 'bearer ' + $scope.token
            }
        })
            .then(function (res) {
                if (res.data.success) {
                    $scope.users = res.data.users;
                }
            })
            .catch(function () {
                $("#errorMessage").text("You have to be authorized to view this content");
            });

    }
    $scope.OnLoad = function () {
        if (($scope.token != null) && ($scope.href.includes("login") || $scope.href.includes("register")))
            window.location.replace("/social/home");
        if ($scope.token == null && !$scope.href.includes("login") && !$scope.href.includes("register"))
            window.location.replace("/social/login");
        if ($scope.href.includes("posts"))
            $scope.GetPosts();
        if (!$scope.href.includes("login") && !$scope.href.includes("register"))
            $scope.GetProfile();
        if ($scope.href.includes("users")) {
            $scope.GetProfile();
            $scope.GetUsers();
        }
        //    if ($scope.href.includes("posts"))
        //        $("#PostsNav").toggleClass("active");
        //    if ($scope.href.includes("users"))
        //        $("#UsersNav").toggleClass("active");
    }
    $scope.OnLoad();
});
app.directive("fileread", [function () {
    return {
        scope: {
            fileread: "="
        },
        link: function (scope, element, attributes) {
            element.bind("change", function (changeEvent) {
                var reader = new FileReader();
                reader.onload = function (loadEvent) {
                    scope.$apply(function () {
                        scope.fileread = loadEvent.target.result;
                    });
                }
                reader.readAsDataURL(changeEvent.target.files[0]);
            });
        }
    }
}]);