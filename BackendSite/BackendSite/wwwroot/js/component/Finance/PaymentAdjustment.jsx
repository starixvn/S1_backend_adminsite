import {number_format} from '../commJS/common';

(function ($, React, ReactDOM, i18n, axios, moment) {
    class PaymentAdjustment extends React.Component {
        constructor(props) {
            super(props);
            this.state={UserName:"", CustData: null};
        }

        handleSubmit(event) {
            event.preventDefault();
            this.onGetCustInfoForPayAdjust();
        }

        onGetCustInfoForPayAdjust(){
            axios.post('/api/CustManage/GetCustInfoForPayAdjust', {CustId:0, UserName: this.state.UserName})
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({CustData: result.data});
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
            if(option==null) {
                alert('Please select one Option.');
                return;
            }

            if(isNaN(amount)) {
                alert('Please select one Bank Care Account.');
                return;
            }     

            axios.post('/api/Payment/PaymentAdjustment', {CustId: custData.custId, Option: parseInt(option), Amount: parseFloat(amount)})
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

        onChange(event) {
            let obj=this.state;
            obj[event.target.name]=event.target.value;
            this.setState(obj);
        }

        render() {
            let custData=this.state.CustData;
            let tpWallet= null;
            if(custData!=null && custData.tpWallet!=null) {
                tpWallet = <ul className="wallet-wrap">{custData.tpWallet.map(function(wallet) { 
                    return <li key={wallet.tpId} className="wallet-item">
                                <span>{wallet.tpName}</span>
                                <span className={wallet.balance <0 ? "negative-num":"positive-num"}>{number_format(wallet.balance)}</span>
                            </li>;
                }.bind(this))}</ul>;
            }
            return (
                <div>
                    <div className="filter">
                        <form method="post" onSubmit={this.handleSubmit.bind(this)}>
                            <div className="row-flex">
                                <div className="input-item">
                                    <label className="col-form-label label">UserName</label>
                                    <input className="form-control" type="text" name="UserName" onChange={this.onChange.bind(this)} />
                                </div>
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
                                        <td>{i18n["lbl_Currency"+custData.currencyId]}</td>
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
                                    <input type="radio" id="Debit" name="Option" className="custom-control-input" value={1} onChange={this.onChange.bind(this)}/>
                                    <label className="custom-control-label" htmlFor="Debit">Debit</label>
                                </div>
                                <div className="custom-control custom-radio custom-control-inline">
                                    <input type="radio" id="Credit" name="Option" className="custom-control-input" value={2} onChange={this.onChange.bind(this)}/>
                                    <label className="custom-control-label" htmlFor="Credit">Credit</label>
                                </div>
                            </div>
                            <div className="form-group width-200">
                                <label className="col-form-label">Amount</label>
                                <input className="form-control" type="number" name="Amount" onChange={this.onChange.bind(this)} step="1" min="1"/>
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