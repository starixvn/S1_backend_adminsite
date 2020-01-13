import {number_format} from '../commJS/common';

(function ($, React, ReactDOM, i18n, axios, moment) {
    class PaymentAdjustment extends React.Component {
        constructor(props) {
            super(props);
            this.state={UserName:"", CustData: null};
        }

        handleSubmit(event) {
            event.preventDefault();
            axios.post('/api/CustManage/GetCustInfoForPayAdjust', {CustId:0, UserName: this.state.UserName})
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({CustData:result.data.custInfoDetail, BankInfoList: result.data.bankInfoList});
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
            if(option==null) {
                alert('Please select one Option.');
                return;
            }
            axios.post('/api/CustManage/', {...custData, Option: parseInt(option) })
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({CustData:result.data});
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));
        }        

        onChange(event) {
            let obj=this.state;
            obj[event.target.name]=event.target.value;
            this.setState(obj);
        }

        render() {
            let custData=this.state.CustData;
            let bankInfoList = this.state.BankInfoList;
            let SelBank = null;
            if(bankInfoList!=null){
                SelBank=<div className="form-group">
                    <label className="col-form-label">Bank</label>
                    <select className="custom-select" onChange={this.onSelSysId.bind(this)} value={this.state.SelSysId}>
                        {this.state.SelSysId==null ? <option>Open this select Bank</option> : null}
                        {BankList.filter(bankInfo => bankInfo.currencyId == selCcyId).map(function(bankInfo) { 
                            return <option key={bankInfo.sysId} value={bankInfo.sysId}>{bankInfo.bankCode + " - " + bankInfo.bankName}</option>;
                        }.bind(this))}
                    </select>
                </div>;
            }
            return (
                <div className="card">
                    <form method="post" className="form-horizontal add-form" onSubmit={this.handleSubmit.bind(this)}>
                        <div className="form-group">
                            <label className="col-form-label">UserName</label>
                            <input className="form-control" type="text" name="UserName" onChange={this.onChange.bind(this)}/>
                        </div>
                        <div className="button-group mt-10">
                            <button type="submit" className="btn btn-primary">Submit</button>
                        </div>                        
                    </form>
                    {custData != null ? 
                    <div className="">
                        <h4 class="page-name">Cust Info</h4>
                        <form method="post" className="form-horizontal add-form" onSubmit={this.onSumbitPaymentAdjustment.bind(this)}>
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
                                        <td>{i18n["lbl_Currency"+custData.currencyId]}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Withdrawable Amount</th>
                                        <td>{number_format(custData.balance) + " (Sports: "+number_format(custData.sportsBalance)+")"}</td>
                                    </tr>
                                </tbody>
                            </table>
                            <div className="form-group">
                                <label className="col-form-label d-block">Option</label>
                                <div className="custom-control custom-radio custom-control-inline">
                                    <input type="radio" id="Debit" name="Option" className="custom-control-input" value={1} onChange={this.onChange.bind(this)}/>
                                    <label className="custom-control-label" htmlFor="Debit">Debit</label>
                                </div>
                                <div className="custom-control custom-radio custom-control-inline">
                                    <input type="radio" id="Credit" name="Option" className="custom-control-input" value={2} onChange={this.onChange.bind(this)}/>
                                    <label className="custom-control-label" htmlFor="Credit">Credit</label>
                                </div>
                            </div>                                           
                            <div className="button-group mt-10">
                                <button type="submit" className="btn btn-primary">Submit</button>
                            </div>                        
                        </form>
                    </div> : null}
                </div>
            )
        }
    }
    
ReactDOM.render(<PaymentAdjustment/>,document.getElementById('App'));
})(jQuery, React, ReactDOM, i18n, axios, moment);