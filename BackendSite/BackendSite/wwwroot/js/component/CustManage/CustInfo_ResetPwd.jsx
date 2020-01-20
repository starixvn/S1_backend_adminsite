import {getUrlParameter} from '../commJS/common';

export class ResetPwd extends React.Component {
    constructor(props) {
        super(props);
        this.state = {NewPwd : null};
    }

    componentDidMount() {
        this._clipboard = new ClipboardJS(".js-copy");

        this._clipboard.on('success', function(e) {
            //console.log(e);
        });
        this._clipboard.on('error', function(e) {
            //console.log(e);
        });   
    }

    onResetPwd(){
        let CustId = getUrlParameter("custId");
        if(!isNaN(CustId)) {
            if(confirm('Are you sure?')) {
                let iCustId = parseInt(CustId);
                axios.post('/api/CustManage/SetResetPwd', {CustId: iCustId})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({NewPwd: result.data});
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
            }
        }
    }

    render() {

        return (
            <div>
                <div className="table-detail">{this.state.NewPwd}{this.state.NewPwd != null ? <button type="button" className="btn btn-copy js-copy" data-clipboard-text={this.state.NewPwd}>Copy</button> : null}</div>
                <div className="button-group"><button className="btn btn-search" onClick={this.onResetPwd.bind(this)}><span>Reset Password</span></button></div>
            </div>
        )
    }
}
