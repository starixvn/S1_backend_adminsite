import {number_format, getUrlParameter} from '../commJS/common';

(function ($, React, ReactDOM, i18n, axios) {
    const CustStatusOp = {
        1:<span className="status status-active">Running</span>,
        2:<span className="status status-suspend">Suspend</span>,
        3:<span className="status status-closed">Closed</span>,
    }   
    const CustActStatusOp = {
        1:<span className="status status-initial">Non-Deposit</span>,
        2:<span className="status status-active">Deposited</span>,
    }           
    class CustInfo extends React.Component {
        constructor(props) {
            super(props);
            this.state = {};
        }

        render() {
            let data = this.props.CustInfoData;
            return (
                data==null ? <div className="text-center">
                    <div className="spinner-border" role="status">
                        <span className="sr-only">Loading...</span>
                    </div>
                </div> : <div>
                    <table class="table table-detail">
                        <tbody>
                            <tr>
                                <th scope="row">Site Name</th>
                                <td>{data.siteName}</td>
                            </tr>
                            <tr>
                                <th scope="row">User Name</th>
                                <td>{data.userName}</td></tr>
                            <tr>
                                <th scope="row">Currency</th>
                                <td>{i18n["lbl_Currency"+data.currencyId]}</td></tr>
                            <tr>
                                <th scope="row">Agent Name</th>
                                <td>{data.agent}</td></tr>
                            <tr>
                                <th scope="row">Prefer Languages</th>
                                <td>{data.userLang}</td>
                            </tr>
                            <tr>
                                <th scope="row">Act Status</th>
                                <td>{CustActStatusOp[data.actStatus]}</td>
                            </tr>
                            <tr>
                                <th scope="row">IsTest</th>
                                <td>{data.isTest ? "True" : "False"}</td></tr>
                            <tr>
                                <th scope="row">Status</th>
                                <td>{CustStatusOp[data.status]}</td>
                            </tr>
                            <tr>
                                <th scope="row">Last Login Time</th>
                                <td>{data.lastLoginTime}</td>
                            </tr>
                            <tr>
                                <th scope="row">Registration Time</th>
                                <td>{data.regTime}</td>
                            </tr>
                            <tr>
                                <th scope="row">First Deposit Time</th>
                                <td>{data.firstDepositTime}</td>
                            </tr>                            
                            <tr>
                                <th scope="row">E-Mail</th>
                                <td>{data.email}</td>
                            </tr>
                            <tr>
                                <th scope="row">Mail Wallet Amount</th>
                                <td>{number_format(data.balance)}</td>
                            </tr>
                            <tr>
                                <th scope="row">Deposit Summary</th>
                                <td>{number_format(data.depositSum)}</td>
                            </tr>
                            <tr>
                                <th scope="row">Withdrawl Summary</th>
                                <td>{number_format(data.withdrawlSum)}</td>
                            </tr>
                            <tr>
                                <th scope="row">Effective Betting Summary</th>
                                <td>{number_format(data.effectiveBetSum)}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            )
        }
    }
    
    class CustInfoPanel extends React.Component {
        _Tabs=["Customer Info","Financial Log","Deposit Info","Transfer Info","Withdrawal Info","Login Log", "Note"];

        constructor(props) {
            super(props);
            this.state = { CustInfoData: null, TabIdx: 0 };
        }

        onTab(idx) {
            this.setState({TabIdx: idx});
        }

        componentDidMount() {
            this.onGetCustInfoDetail();
        }

        onGetCustInfoDetail(){
            let CustId = getUrlParameter("custId");
            if(!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                axios.post('/api/CustManage/GetCustInfoDetail', {CustId: iCustId})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({CustInfoData: result.data});
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
            }
        }

        render() {
            let dataContainer = null;
            let TabIdx = this.state.TabIdx;
            if(TabIdx == 0){
                dataContainer = <CustInfo CustInfoData={this.state.CustInfoData}/>
            } else {

            }

            var tabBtn = this._Tabs.map(function(item, index){
                return <li className="nav-item" key={index}>
                <a className={"nav-link"+ (index==TabIdx? " active":"")} href="javascript:void(0);" onClick={index==TabIdx ? null: this.onTab.bind(this, index)}>{item}</a>
            </li>;
            }.bind(this));                        

            return (
                <div>
                    <h4 className="page-title">Customer Information</h4>
                    <div className="p-20">
                        <div className="row">
                            <div className="info-tab">
                                <div className="nav flex-column nav-pills" id="v-pills-tab" role="tablist" aria-orientation="vertical">
                                    <div className="user">
                                        <img className="avatar" src="/images/avatar.png" alt="User image" />
                                        <span className="name">Luke Davids</span>
                                    </div>
                                    {tabBtn}
                                </div>
                            </div>
                            <div className="info-content">
                                <div className="tab-content" id="v-pills-tabContent">
                                    {dataContainer}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>                
            )
        }
    }

    ReactDOM.render(<CustInfoPanel/>,document.getElementById('Panel'));
})(jQuery, React, ReactDOM, i18n, axios);