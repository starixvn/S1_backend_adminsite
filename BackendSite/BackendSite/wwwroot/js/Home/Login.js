$(function () {
    $("#frmLogin").submit(function (event) {
        event.preventDefault();
        var obj;
        obj = document.getElementById('txtID');
        if (obj.value == '') {
            obj.focus();
            alert(i18n.errlogin_enter_un);
            return;
        }

        obj = document.getElementById('txtPW');
        if (obj.value == '') {
            obj.focus();
            alert(i18n.errlogin_enter_pw);
            return;
        }

        const urlParams = new URLSearchParams(window.location.search);
        const redirectUrl = urlParams.get('redirectUrl');
     
        var data = {txtID:$("#txtID").val(),txtPW:$("#txtPW").val(),redirectUrl:redirectUrl};
        axios.post('/api/Home/Signin',data)
        .then(function (response) {
            let result = response.data;
            if (result.errorCode == 0) {
                window.location.href=`${window.location.origin}/${result.message==='/' ? '' :result.message }`
            } else {
                alert(result.message);
            }
        }).catch(function (error) {
            alert(error);
        });
    });
});