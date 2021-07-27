import { number_format } from '../commJS/common';
import qs from 'qs';

(function ($, React, ReactDOM, i18n, axios, moment) {
    class PaymentAdjustment extends React.Component {
        constructor(props) {
            super(props);
            this.state = { UserName: "", CustData: null, Checkwd: false, Withdrawal: 1, SiteList: [], SiteId: null, SabaId: "", AdjustMentLimitMax:0, CheckPassword:false};
        }

        componentDidMount = () => {
            this.getSiteList();
        }

        handleSubmit(event) {
            event.preventDefault();
            this.onGetCustInfoForPayAdjust();
        }

        getSiteList() {
            //Loading.OnLoading();
            axios.post('/api/Common/AdminUserSiteList')
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({ SiteList: result.data });
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
        }

        onSiteChanged(e) {
            let siteId = parseInt(e.target.value);
            let SelSite = null;
            for (let i = 0; i < this.state.SiteList.length; i++) {
                let site = this.state.SiteList[i];
                if (site.siteId == siteId) {
                    SelSite = site;
                    break;
                }
            }

            this.setState({ SiteId: siteId, SelSite: SelSite });
        }

        onGetCustInfoForPayAdjust() {
            let msg = "";
            let check = true;

            if (!this.state.SabaId && !this.state.UserName) {
                check = false;
                msg = "Please input SabaId or UserName.";
            }

            if (!this.state.SiteId) {
                check = false;
                msg = "Please select Site.";
            }
           
            if (check) {
                axios.post('/api/CustManage/GetCustInfoDetails', { SabaId: this.state.SabaId, UserName: this.state.UserName, SiteId: this.state.SiteId })
                    .then(function (response) {
                        //Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ CustData: result.data });
                            this.GetAdjustmentLimitMAX(result.data.siteId,result.data.currencyId);
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        //Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
            else {
                alert(msg);
            }
        }

        GetAdjustmentLimitMAX(siteId,currencyId){
            axios.post('/api/Payment/GetAdjustmentData', { siteId: siteId, currencyId: currencyId})
                    .then(function (response) {
                        //Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            //this.setState({ CustData: result.data });
                            if(result.data){
                                this.setState({ AdjustMentLimitMax: result.data.adjustmentMax });
                            }
                            else{
                                this.setState({ AdjustMentLimitMax: 0 });
                            }
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        //Loading.UnLoading();
                        alert(error);
                    }.bind(this));
        }

        onSumbitPaymentAdjustment(event) {
            event.preventDefault();
            let custData = this.state.CustData;
            let option = this.state.Option;
            let amount = this.state.Amount;
            let adjustMentLimitMax = this.state.AdjustMentLimitMax;
            let checkwd = (this.state.Checkwd) ? 1 : 0;
            let withdrawal = parseFloat(this.state.Withdrawal);
            let withdrawalamount = isNaN(parseFloat(amount) * withdrawal) ? 0 : parseFloat(amount) * withdrawal;
            let memo = this.state.Memo;

            if (option == null) {
                alert('Please select one Option.');
                return;
            }

            if (isNaN(amount)) {
                alert('Please select one Bank Care Account.');
                return;
            }

            if (parseInt(option) === 1 && checkwd === 1 && withdrawalamount === 0) {
                alert('Please Enter Withdrawal Limit');
                return;
            }

            if(amount > adjustMentLimitMax && adjustMentLimitMax !== 0 && (parseInt(option) === 1 || parseInt(option) === 3)){
                this.setState({ CheckPassword: true });
                return;
            }

            axios.post('/api/Payment/AdjustmentAndWithdrawalLimit', { CustId: custData.custId, Option: parseInt(option), Amount: parseFloat(amount), WithdrawalLimitOption: checkwd, WithdrawalLimitAmount: withdrawalamount, Memo: memo })
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.onGetCustInfoForPayAdjust();
                        alert(result.message);
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
        }

        onSumbitPaymentAdjustmentByPassWord(){
            let custData = this.state.CustData;
            let option = this.state.Option;
            let amount = this.state.Amount;
            let withdrawal = parseFloat(this.state.Withdrawal);
            let checkwd = (this.state.Checkwd) ? 1 : 0;
            let withdrawalamount = isNaN(parseFloat(amount) * withdrawal) ? 0 : parseFloat(amount) * withdrawal;
            let memo = this.state.Memo;
            axios.post('/api/Payment/AdjustmentAndWithdrawalLimit', { CustId: custData.custId, Option: parseInt(option), Amount: parseFloat(amount), WithdrawalLimitOption: checkwd, WithdrawalLimitAmount: withdrawalamount, Memo: memo })
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.onGetCustInfoForPayAdjust();
                    this.ClickCancel();
                    alert(result.message);
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));
        }

        onChange(event) {
            let obj = this.state;
            obj[event.target.name] = event.target.value;
            this.setState(obj);
        }

        onCheck(Checkwd) {
            this.setState({ Checkwd: !Checkwd });
        }

        ClickCancel(){
            this.setState({ CheckPassword: false });
        }

        render() {
            let custData = this.state.CustData;
            let tpWallet = null;
            if (custData != null && custData.tpWallet != null) {
                tpWallet = <ul className="wallet-wrap">{custData.tpWallet.map(function (wallet) {
                    return <li key={wallet.tpId} className="wallet-item">
                        <span>{wallet.tpName}</span>
                        <span className={wallet.balance < 0 ? "negative-num" : "positive-num"}>{number_format(wallet.balance)}</span>
                    </li>;
                }.bind(this))}</ul>;
            }


            let siteList = this.state.SiteList.map(function (obj) {
                return <div key={obj.siteId} className="custom-control custom-radio custom-control-inline">
                    <input type="radio" id={"SiteId_" + obj.siteId} name="SiteList" className="custom-control-input" checked={this.state.SiteId === obj.siteId} value={obj.siteId} onChange={this.onSiteChanged.bind(this)} />
                    <label className="custom-control-label" htmlFor={"SiteId_" + obj.siteId}>{obj.siteName}</label>
                </div>;
            }.bind(this));

            return (
                <div>
                    <div className="filter">
                        <form method="post" onSubmit={this.handleSubmit.bind(this)}>
                            <div className="form-group">
                                <label className="col-form-label d-block">Site Name</label>
                                {siteList}
                            </div>
                            <div className="input-item">
                                <label className="col-form-label label">UserName</label>
                                <input className="form-control" style={{ 'width': '250px' }} type="text" name="UserName" onChange={this.onChange.bind(this)} />
                            </div>
                            <div className="input-item">
                                <label className="col-form-label label">SabaId</label>
                                <input className="form-control" style={{ 'width': '250px' }} type="text" name="SabaId" onChange={this.onChange.bind(this)} />
                            </div>
                            <div className="row-flex">
                                <div className="button-group">
                                    <button type="submit" className="btn btn-primary">Submit</button>
                                </div>
                            </div>
                        </form>
                    </div>
                    {custData != null ?
                        <div>
                            <h4 className="page-name">Cust Info</h4>
                            <form method="post" onSubmit={this.onSumbitPaymentAdjustment.bind(this)}>
                                <table className="table table-detail">
                                    <tbody>
                                        <tr>
                                            <th scope="row">SiteName</th>
                                            <td>{custData.siteName}</td>
                                        </tr>
                                        <tr>
                                            <th scope="row">User Name</th>
                                            <td>{custData.userName}</td>
                                        </tr>
                                        <tr>
                                            <th scope="row">Currency</th>
                                            <td>{i18n["lbl_Currency" + custData.currencyId]}</td>
                                        </tr>
                                        <tr>
                                            <th scope="row">Withdrawable Amount</th>
                                            <td>{number_format(custData.balance)}</td>
                                        </tr>
                                    </tbody>
                                </table>
                                {tpWallet}
                                <div className="form-group">
                                    <label className="col-form-label d-block">Option</label>
                                    <div className="custom-control custom-radio custom-control-inline">
                                        <input type="radio" id="Option_1" name="Option" className="custom-control-input" value={1} onChange={this.onChange.bind(this)} />
                                        <label className="custom-control-label" htmlFor="Option_1">Balance(+)</label>
                                    </div>
                                    <div className="custom-control custom-radio custom-control-inline">
                                        <input type="radio" id="Option_2" name="Option" className="custom-control-input" value={2} onChange={this.onChange.bind(this)} />
                                        <label className="custom-control-label" htmlFor="Option_2">Balance(-)</label>
                                    </div>
                                    <div className="custom-control custom-radio custom-control-inline">
                                        <input type="radio" id="Option_3" name="Option" className="custom-control-input" value={3} onChange={this.onChange.bind(this)} />
                                        <label className="custom-control-label" htmlFor="Option_3">Bonus(+)</label>
                                    </div>
                                    <div className="custom-control custom-radio custom-control-inline">
                                        <input type="radio" id="Option_4" name="Option" className="custom-control-input" value={4} onChange={this.onChange.bind(this)} />
                                        <label className="custom-control-label" htmlFor="Option_4">Bonus(-)</label>
                                    </div>
                                </div>
                                <div className="form-group">
                                    <div className="custom-control custom-checkbox custom-control-inline">
                                        <input className="custom-control-input" id="checkwd" type="checkbox" name="Checkwd" checked={this.state.Checkwd} onChange={this.onCheck.bind(this, this.state.Checkwd)} />
                                        <label className="custom-control-label" htmlFor="checkwd">Withdraw Condition(Balance Amount X Times):</label>
                                    </div>
                                    <input className="form-control width-200" type="number" name="Withdrawal" pattern="[0-9]*" value={this.state.Withdrawal} onChange={this.onChange.bind(this)}/>
                                </div>
                                <div className="form-group width-200">
                                    <label className="col-form-label">Amount</label>
                                    <input className="form-control" type="number" name="Amount" onChange={this.onChange.bind(this)} step="1" min="1" />
                                </div>
                                <div className="form-group width-200">
                                    <label className="col-form-label">Memo</label>
                                    <textarea className="form-control" name="Memo" onChange={this.onChange.bind(this)} />
                                </div>
                                <div className="button-group mt-10">
                                    <button type="submit" className="btn btn-primary">Submit</button>
                                </div>
                            </form>
                        {this.state.CheckPassword ? <PopBoxContent 
                                                    currency={i18n["lbl_Currency" + this.state.CustData.currencyId]} 
                                                    amount={this.state.Amount}
                                                    ClickCancel={this.ClickCancel.bind(this)}
                                                    onSumbitPaymentAdjustmentByPassWord={this.onSumbitPaymentAdjustmentByPassWord.bind(this)}
                                                    SiteId={this.state.SiteId}
                                                    />:null}
                        </div> : null}
                </div>
            )
        }
    }

    class PopBoxContent extends React.Component {
        constructor(props) {
            super(props);
            this.state = { password: ""};
        }
        onChange(event) {
            this.setState({
                password:event.target.value
            })
        }
        handleSave(){
            axios.post('/api/Payment/CheckPassWord', { PassWord: this.state.password ,SiteId:this.props.SiteId})
                .then(function (response) {
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.props.onSumbitPaymentAdjustmentByPassWord();
                    } else {
                        alert('Password Error');
                    }
                }.bind(this)).catch(function (error) {
                    alert(error);
                }.bind(this));
        }
        render() {
            let currency = this.props.currency;
            let amount = this.props.amount;
            return (
                <div className="newwrapper">
                    <div className="popup-wrap is-show">
                        <div className="popup is-open">
                            <div className="popup__head">
                                <span className="text">Please kyin password to confirm the amount-{currency}:{amount} </span>
                                {/* <span className="text">{currency} {amount}</span> */}
                                <div className="newbtn"><i className="icon icon-close"></i></div>
                            </div>
                            <div className="popup__body os-host os-theme-dark os-host-resize-disabled os-host-scrollbar-horizontal-hidden os-host-scrollbar-vertical-hidden os-host-transition">
                                <div className="os-resize-observer-host">
                                    <div className="os-resize-observer observed"></div>
                                </div>
                                <div className="os-size-auto-observer">
                                    <div className="os-resize-observer observed"></div>
                                </div>
                                <div className="os-content-glue"></div>
                                <div className="os-padding">
                                    <div className="os-viewport os-viewport-native-scrollbars-invisible">
                                        <div className="os-content">
                                            <div className="popup__row">
                                                <div className="form">
                                                    <div className="form__group">
                                                        <div className="form__title">
                                                            <span className="text">password</span>
                                                        </div>
                                                        <div className="textfield">
                                                            <input  defaultValue="" onChange={this.onChange.bind(this)} />
                                                        </div>
                                                    </div>                                                  
                                                </div>
                                            </div>
                                            <div className="popup__row">
                                                <div className="newbtn-bar">
                                                    <div className="newbtn-bar__group">
                                                        <div className="newbtn newbtn-primary newbtn-w-min" onClick={this.handleSave.bind(this)}><span className="text">OK</span></div>
                                                        <div className="newbtn newbtn-secondary newbtn-w-min" onClick={this.props.ClickCancel}><span className="text">Cancel</span></div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div className="os-scrollbar os-scrollbar-horizontal os-scrollbar-unusable">
                                    <div className="os-scrollbar-track os-scrollbar-track-off">
                                        <div className="os-scrollbar-handle">
                                        </div>
                                    </div>
                                </div>
                                <div className="os-scrollbar os-scrollbar-vertical os-scrollbar-unusable">
                                    <div className="os-scrollbar-track os-scrollbar-track-off">
                                        <div className="os-scrollbar-handle">
                                        </div>
                                    </div>
                                </div>
                                <div className="os-scrollbar-corner"></div>
                            </div>
                        </div>
                    </div>
                </div>
            )
        }
    }
    ReactDOM.render(<PaymentAdjustment />, document.getElementById('App'));
})(jQuery, React, ReactDOM, i18n, axios, moment);