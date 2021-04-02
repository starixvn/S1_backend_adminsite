import DateTimePicker from "react-datetime";
import React from 'react';
import { Dropdown } from 'semantic-ui-react';

(function($, React, ReactDOM, i18n, axios, moment) {

    class DepositSubmissionForm extends React.Component {
        constructor(props) {
            super(props);

            this.state = {
                SiteList: [],
                SiteId: 0,
                UserName: null,
                Amount: 0.00,
                BankName: null,
                AccountName: "",
                AccountNo: "",
                ReferenceNumber: null,
                Remark: null,
                File: null,
                selectOptions: [],
                BankCode: "",
                SysId: 0
            };

            this.DepositTime = React.createRef();
        }

        componentDidMount() {
            this.getSiteList();
            this.getAccountList();
        }

        onChange(event) {
            let obj = this.state;
            obj[event.target.name] = event.target.value;
            this.setState(obj);
        }

        setFile(e) {
            this.setState({ File: e.target.files[0] });
        }

        getSiteList() {
            axios.post('/api/Common/AdminUserSiteList')
                .then(function (response) {
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({
                            SiteList: result.data,
                            SiteId: result.data[0].siteId
                        });
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    alert(error);
                }.bind(this));
        }

        async getAccountList() {
            const res = await axios.get('/api/BankList/GetAllBankInfoOffList');
            const data = res.data.data;

            const options = data.map(d => ({
                "key": d.accountNo + d.sysId,
                "value": d.accountNo,
                "text": d.accountNo + '   (' + d.bankCode + ' - ' + d.accountName + ') ',
                "accountname": d.accountName,
                "accountno": d.accountNo,
                "accountstatus": d.accountStatus,
                "sysid": d.sysId,
                "currencyid": d.currencyId,
                "bankcode": d.bankCode,
                "bankname": d.bankName,
                "bankstatus": d.bankStatus
            }));

            this.setState({ selectOptions: options });
        }

        onSiteChanged(e) {
            let siteId = parseInt(e.target.value);
            this.setState({ SiteId: siteId });
        }

        onSelectedAccount(e) {
            if (e.keyCode != '38' && e.keyCode != '40') {
                this.setState({
                    BankName: e.currentTarget.getAttribute("bankname"),
                    AccountName: e.currentTarget.getAttribute("accountname"),
                    AccountNo: e.currentTarget.getAttribute("accountno"),
                    SysId: parseInt(e.currentTarget.getAttribute("sysid"))
                });
            }
            if (e.keyCode == '38' || e.keyCode == '40') {
                this.setState({
                    BankName: e.target.getAttribute("bankname"),
                    AccountName: e.target.getAttribute("accountname"),
                    AccountNo: e.target.getAttribute("accountno"),
                    SysId: parseInt(e.target.getAttribute("sysid"))
                });
            }
        }

        onSubmitDeposit(event) {
            let deposit = {
                SiteId: this.state.SiteId,
                UserName: this.state.UserName,
                Amount: parseFloat(this.state.Amount),
                Comment: this.state.Comment,
                RefId: this.state.ReferenceNumber,
                DepositTime: moment(this.DepositTime.current.state.inputValue).unix(),
                Operator: "admin chipmunk",
                BankName: this.state.BankName,
                AccountHolderName: this.state.AccountName,
                AccountNumber: this.state.AccountNo,
                SysId: this.state.SysId
            };

            var bodyFormData = new FormData();
            bodyFormData.append("DepositDetails", JSON.stringify(deposit));
            bodyFormData.append("Receipt", this.state.File);

            axios({
                    method: "post",
                    url: "/api/Deposit/DepositSubmission",
                    data: bodyFormData,
                    headers: { 'Content-Type': "multipart/form-data" }
                })
                .then(function(response) {
                    console.log(response.data.errorCode);
                    console.log(response.data.message);
                    alert('test');
                });
        }

        render() {

            let siteList = this.state.SiteList.map(function(obj) {
                return <div key={obj.siteId} className="custom-control custom-radio custom-control-inline">
                    <input type="radio" id={"SiteId_" + obj.siteId} name="SiteList" className="custom-control-input" value={obj.siteId} onChange={this.onSiteChanged.bind(this)} defaultChecked={this.state.SiteId == obj.siteId}/>
                            <label className="custom-control-label" htmlFor={"SiteId_" + obj.siteId}>{obj.siteName}</label>
                       </div>;
            }.bind(this));

            return (
                <div className="card">
                    
                    <form method="post" className="form-horizontal add-form" onSubmit={this.onSubmitDeposit.bind(this)}>
                        <div className="form-group">
                            <label className="col-form-label d-block">Site Name</label>
                            {siteList}
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">Username</label>
                            <input className="form-control" type="text" name="UserName" onChange={this.onChange.bind(this)}/>
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">Amount</label>
                            <input className="form-control text-right" type="text" name="Amount" onChange={this.onChange.bind(this)}/>
                        </div>
                        <div>--------------------------------------------------------------------------</div>
                        <div className="form-group">
                            <label className="col-form-label">Account No</label>
                            <Dropdown
                                clearable
                                fluid
                                search
                                selection
                                options={this.state.selectOptions}
                                placeholder='Deposited Account Number'
                                onChange={this.onSelectedAccount.bind(this)}
                                /*onKeyPress={(e) => { e.key === 'Enter' && e.preventDefault(); }}*/
                                />
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">Bank Name</label>
                            <input className="form-control" type="text" name="BankName" value={this.state.BankName || ""} onChange={this.onChange.bind(this)}/>
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">Account Holder Name</label>
                            <input className="form-control" type="text" name="AccountName" value={this.state.AccountName || ""} onChange={this.onChange.bind(this)}/>
                        </div>
                            <div>--------------------------------------------------------------------------</div>
                        <div className="form-group">
                            <label className="col-form-label">Reference No</label>
                            <input className="form-control" type="text" name="ReferenceNumber" onChange={this.onChange.bind(this)}/>
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">Submitted DateTime</label>
                            <div className="date">
                                <DateTimePicker
                                    columnTitle="SubmittedDateTime"
                                    ref={this.DepositTime}
                                    dateTimePickerProps={{
                                        defaultValue: moment().format(
                                            "l LTS"
                                        )
                                    }}/>
                            </div>
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">Comment</label>
                            <input className="form-control" type="text" name="Comment" onChange={this.onChange.bind(this)}/>
                        </div>
                        <div>
                            <label className="col-form-label">Receipt</label>
                            <span>
                                <input id="uploadReceipt" className="form-control" type="file" accept=".png, .jpg, .jpeg" onChange={e => this.setFile(e)}/>
                            </span>
                        </div>
                        <div className="button-group mt-10">
                            <button type="submit" className="btn btn-primary">Submit</button>
                        </div>
                    </form>
                </div>
            );
        }
    }

    ReactDOM.render(<DepositSubmissionForm/>, document.getElementById("DepositSubmissionForm"));

})(jQuery, React, ReactDOM, i18n, axios, moment);