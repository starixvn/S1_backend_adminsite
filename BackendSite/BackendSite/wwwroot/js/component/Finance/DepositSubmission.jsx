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
                UserName: "",
                Amount: 0,
                BankName: null,
                AccountName: "",
                AccountNo: "",
                ReferenceNumber: null,
                Remark: "",
                File: null,
                selectOptions: [],
                BankCode: "",
                CurrencyId: 0,
                SysId: 0
            };

            this.DepositTime = React.createRef();
        }

        componentDidMount() {
            this.getSiteList();
            this.getAccountList();
        }

        onChange(e) {
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
                    SysId: parseInt(e.currentTarget.getAttribute("sysid")),
                    CurrencyId: parseInt(e.currentTarget.getAttribute("currencyid"))
                });
            }
        }

        onSubmitDeposit(event) {
            event.preventDefault();
            if (this.state.UserName.length == 0) {
                alert("Please enter username");
                return;
            }

            if (this.state.Amount == 0 || isNaN(parseFloat(this.state.Amount))) {
                alert("Please enter deposited amount");
                this.setState({
                    Amount: 0
                })
                return;
            }

            if (this.state.SysId.length == 0 || this.state.SysId == 0 || this.state.SysId == null) {
                alert("Please select deposited bank");
                return;
            }

            if (this.DepositTime.current.state.inputValue == "") {
                alert("Please select proper time");
                return;
            }

            let deposit = {
                SiteId: this.state.SiteId,
                UserName: this.state.UserName,
                Amount: parseFloat(this.state.Amount) || 0,
                Comment: this.state.Remark,
                RefId: this.state.ReferenceNumber,
                DepositTime: moment(this.DepositTime.current.state.inputValue).unix(),
                BankName: this.state.BankName,
                AccountHolderName: this.state.AccountName,
                AccountNumber: this.state.AccountNo,
                SysId: this.state.SysId,
                CurrencyId: this.state.CurrencyId
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
                .then((response) => {
                    if (response.data.errorCode === 1) {
                        this.setState({
                            UserName: "",
                            Amount: 0.00,
                            BankName: null,
                            AccountName: "",
                            AccountNo: "",
                            ReferenceNumber: null,
                            Remark: "",
                            SysId: 0, 
                            File: null
                        });
                        document.getElementById('uploadReceipt').value = '';
                    }
                    alert(response.data.message);
                }).catch(function (error) {
                    alert(error);
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
                            <input className="form-control" type="text" name="UserName" onChange={this.onChange.bind(this)} value={this.state.UserName || ""}/>
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">Amount</label>
                            <input className="form-control text-right" type="number" name="Amount" onChange={this.onChange.bind(this)} value={this.state.Amount || ""}/>
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
                                value={this.state.AccountNo || ''}
                                /*onKeyPress={(e) => { e.key === 'Enter' && e.preventDefault(); }}*/
                                />
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">Bank Name</label>
                            <input className="form-control" type="text" name="BankName" value={this.state.BankName || ""} onChange={this.onChange.bind(this)} disabled="disabled"/>
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">Account Holder Name</label>
                            <input className="form-control" type="text" name="AccountName" value={this.state.AccountName || ""} onChange={this.onChange.bind(this)} disabled="disabled"/>
                        </div>
                            <div>--------------------------------------------------------------------------</div>
                        <div className="form-group">
                            <label className="col-form-label">Reference No</label>
                            <input className="form-control" type="text" name="ReferenceNumber" onChange={this.onChange.bind(this)} value={this.state.ReferenceNumber || ""}/>
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
                            <label className="col-form-label">Remark</label>
                            <input className="form-control" type="text" name="Remark" onChange={this.onChange.bind(this)} value={this.state.Remark || ""}/>
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