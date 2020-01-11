import {number_format} from '../commJS/common';

(function ($, React, ReactDOM, i18n, axios, moment) {
    const BankCardStatus = [<span className="status status-initial">Initial</span>,
                            <span className="status status-active">Active</span>,
                            <span className="status status-suspend">Suspend</span>,
                            <span className="status status-closed">Closed</span>];
    const Purpose = {1:<span>Deposit</span>,
                    2:<span>Withdrawal</span>,
                    3:<span>Deposit & Withdrawal</span>};                     

    class BankCardInfoList extends React.Component {
        constructor(props) {
            super(props);

        }

        onRefresh() {
            this.props.onRefresh();
        }

        render() {
            let rows = this.props.searchData.map(row => 
                <tr key={row.bankId}>
                    <th scope="row">{row.bankCode}</th>
                    <td>{row.bankName}</td>
                    <td>{row.accountNo}</td>
                    <td>{row.accountName}</td>                    
                    <td>{row.pointLevel}</td>                    
                    <td className="text-right">{number_format(row.minAmount)}</td>
                    <td className="text-right">{number_format(row.maxAmount)}</td>
                    <td className="text-right">{number_format(row.dailyLimitAmount)}</td>
                    <td>{Purpose[row.purpose]}</td>
                    <td>{BankCardStatus[row.status]}</td>
                    <td className="text-right">{number_format(row.dummy)}</td>
                </tr>
            );

            return (
                <div className="table">
                    <div className="table-options">
                        <button type="button" className="btn btn-refresh" onClick={this.onRefresh.bind(this)}>                                    
                            Refresh
                        </button>
                    </div>
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                <th scope="col">Bank Code</th>
                                <th scope="col">Bank Name</th>
                                <th scope="col">Acc. NO.</th>
                                <th scope="col">Acc. Name</th>
                                <th scope="col">VIP Level</th>                                
                                <th scope="col" className="text-right">Min Amount</th>
                                <th scope="col" className="text-right">Max Amount</th>
                                <th scope="col" className="text-right">Daily Limit Amount</th>
                                <th scope="col">Purpose</th>
                                <th scope="col">Status</th>
                                <th scope="col" className="text-right">Dummy</th>
                            </tr>
                        </thead>
                        <tbody>
                            {rows}
                        </tbody>
                    </table>
                </div>
            )
        }
    }

    class CcyPanel extends React.Component {
        constructor(props) {
            super(props);

        }

        onRefresh() {
            this.props.onRefresh();
        }

        showForm(currencyId) {
            this.props.showForm(currencyId);
        }

        render() {
            return (
            <div className="panel mt-20">
                <a className="panel-title" data-toggle="collapse" href={"#collapseExample-"+this.props.currencyId} role="button" aria-expanded="true" aria-controls="collapseExample">
                  {i18n["lbl_Currency"+this.props.currencyId]}
                </a>
                <button className="btn btn-add" onClick={this.showForm.bind(this, this.props.currencyId)}>Add Bank Card</button>
                <div className="panel-body">
                    <div className="collapse show" id={"collapseExample-"+this.props.currencyId}>
                        <BankCardInfoList searchData={this.props.searchData}/>          
                    </div>
                </div>
            </div>
            )
        }
    }    

    class BankCardForm extends React.Component {
        constructor(props) {
            super(props);

            this.state={BankList:null, SelBank:null, SelCcyId: this.props.SelCcyId,SelSysId:null,Status:0, Sort:0};
        }

        closeForm() {
            this.props.closeForm();
        }

        onSiteChanged(e) {
            this.setState({SelBank:null, SelCcyId:null});
            this.props.onSiteChanged(e);
        }

        handleSubmit(event) {
            event.preventDefault();
            let SelSite=this.props.SelSite;
            let thisState = this.state;

            if(SelSite==null) {
                alert('Please Select Site');
                return;
            }
            
            if(thisState.SelSysId==null) {
                alert('Please Select Bank');
                return;
            }            

            if(thisState.BankAccountName==null) {
                alert('Please Select Account Name');
                return;
            }

            if(thisState.BankAccountNo==null) {
                alert('Please Select Account No');
                return;
            }

            if(isNaN(thisState.MaxAmount)) {
                alert('Please Input MaxAmount');
                return;
            }

            if(isNaN(thisState.MinAmount)) {
                alert('Please Input MaxAmount');
                return;
            }

            if(isNaN(thisState.DailyLimitAmount)) {
                alert('Please Input Daily Limit Amount');
                return;
            }

            if(isNaN(thisState.PointLevel)) {
                alert('Please Select Vip Level');
                return;
            }

            if(isNaN(thisState.Purpose)) {
                alert('Please Select Purpose');
                return;
            }            

            let data = {siteId:SelSite.siteId,SysId: thisState.SelSysId,BankAccountName: thisState.BankAccountName, BankAccountNo: thisState.BankAccountNo, MaxAmount: parseFloat(thisState.MaxAmount), MinAmount: parseFloat(thisState.MinAmount), DailyLimitAmount: parseFloat(thisState.DailyLimitAmount), PointLevel: parseInt(thisState.PointLevel), Purpose: parseInt(thisState.Purpose), Status: thisState.Status,Sort: thisState.Sort}

            axios.post('/api/BankList/SetBankCard', data)
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    alert(result.message);
                    this.props.onSaveForm();
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));
        }

        onSelCcyId(e) {
            this.setState({SelCcyId: parseInt(event.target.value),SelSysId:null});
        }

        onSelSysId(e) {
            this.setState({SelSysId: parseInt(event.target.value)});
        }

        getSiteList() {
            //Loading.OnLoading();
            axios.post('/api/Home/GetSiteList')
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({SiteList: result.data});
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));
        }

        onChange(event) {
            let obj={};
            obj[event.target.name]=event.target.value;
            this.setState(obj);
        }

        render() {
            //let searchData = this.state.searchData;
            let siteList = this.props.SiteList.map(function(obj) { 
                return <div key={obj.siteId} className="custom-control custom-radio custom-control-inline">
                            <input type="radio" id={"SiteId_"+obj.siteId} name="SiteList" className="custom-control-input" checked={this.props.SiteId === obj.siteId} value={obj.siteId} onChange={this.onSiteChanged.bind(this)}/>
                            <label className="custom-control-label" htmlFor={"SiteId_"+obj.siteId}>{obj.siteName}</label>
                        </div>;
            }.bind(this));
            let SelSite=this.props.SelSite;
            let BankList = this.props.SysBankList;
            let CcyOp = null;
            let SelBank = null;            
            if(SelSite != null) {
                CcyOp = SelSite.iCurrencyList.map(function(currencyId) { 
                        return <option key={currencyId} value={currencyId}>{i18n["lbl_Currency"+currencyId]}</option>;
                    }.bind(this));
            }

            if(this.state.SelCcyId !=null) {
                let selCcyId = this.state.SelCcyId;
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
                            <label className="col-form-label d-block">Site Name</label>
                            {siteList}
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">CCY</label>
                            <select className="custom-select" onChange={this.onSelCcyId.bind(this)} value={this.state.SelCcyId}>
                                {this.state.SelCcyId==null ? <option>Open this select Currency</option> : null}
                                {CcyOp}
                            </select>
                        </div>
                        {SelBank}
                        <div className="form-group">
                            <label className="col-form-label">Account Name</label>
                            <input className="form-control" type="text" name="BankAccountName" onChange={this.onChange.bind(this)}/>
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">Account No</label>
                            <input className="form-control" type="text" name="BankAccountNo" onChange={this.onChange.bind(this)}/>
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">Min Amount (0 is not checked)</label>
                            <input className="form-control" type="number" name="MinAmount" onChange={this.onChange.bind(this)}/>
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">Max Amount (0 is not checked)</label>
                            <input className="form-control" type="number" name="MaxAmount" onChange={this.onChange.bind(this)}/>
                        </div>
                        <div className="form-group">
                            <label className="col-form-label">Daily Limit Amount (0 is not checked)</label>
                            <input className="form-control" type="number" name="DailyLimitAmount" onChange={this.onChange.bind(this)}/>
                        </div>                        
                        <div className="form-group">
                            <label className="col-form-label d-block">Point Level</label>
                            <div className="custom-control custom-radio custom-control-inline">
                                <input type="radio" id="VipLevel0" name="PointLevel" className="custom-control-input" value={0} onChange={this.onChange.bind(this)}/>
                                <label className="custom-control-label" htmlFor="VipLevel0">0 (Non-Deposit)</label>
                            </div>
                            <div className="custom-control custom-radio custom-control-inline">
                                <input type="radio" id="VipLevel1" name="PointLevel" className="custom-control-input" value={1} onChange={this.onChange.bind(this)}/>
                                <label className="custom-control-label" htmlFor="VipLevel1">1</label>
                            </div>
                            <div className="custom-control custom-radio custom-control-inline">
                                <input type="radio" id="VipLevel2" name="PointLevel" className="custom-control-input" value={2} onChange={this.onChange.bind(this)}/>
                                <label className="custom-control-label" htmlFor="VipLevel2">2</label>
                            </div>
                            <div className="custom-control custom-radio custom-control-inline">
                                <input type="radio" id="VipLevel3" name="PointLevel" className="custom-control-input" value={3} onChange={this.onChange.bind(this)}/>
                                <label className="custom-control-label" htmlFor="VipLevel3">3</label>
                            </div>
                        </div>
                        <div className="form-group">
                            <label className="col-form-label d-block">Purpose</label>
                            <div className="custom-control custom-radio custom-control-inline">
                                <input type="radio" id="Deposit" name="Purpose" className="custom-control-input" value={1} onChange={this.onChange.bind(this)}/>
                                <label className="custom-control-label" htmlFor="Deposit">Deposit</label>
                            </div>
                            <div className="custom-control custom-radio custom-control-inline">
                                <input type="radio" id="Withdrawal" name="Purpose" className="custom-control-input" value={2} onChange={this.onChange.bind(this)}/>
                                <label className="custom-control-label" htmlFor="Withdrawal">Withdrawal</label>
                            </div>
                            <div className="custom-control custom-radio custom-control-inline">
                                <input type="radio" id="DepositWithdrawal" name="Purpose" className="custom-control-input" value={3} onChange={this.onChange.bind(this)}/>
                                <label className="custom-control-label" htmlFor="DepositWithdrawal">Deposit & Withdrawal</label>
                            </div>
                        </div>                        
                        <div className="button-group mt-10">
                            <button type="button" className="btn btn-back" onClick={this.closeForm.bind(this)}>Back</button>
                            <button type="submit" className="btn btn-primary">Create</button>
                        </div>
                    </form>
                </div>
            )
        }
    }        

    class BankCardInfoPanel extends React.Component {
        constructor(props) {
            super(props);

            this.state={SiteId: null, ShowForm: false,SiteList:[], SelSite:null};
        }

        onRefresh() {
            this.onSearch();
        }

        onChange(event) {
            let obj={};
            obj[event.target.name]=event.target.value;
            this.setState(obj);
        }

        getSiteList() {
            //Loading.OnLoading();
            axios.post('/api/Home/GetSiteList')
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({SiteList: result.data});
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));
        }

        getSysBankList() {
            //Loading.OnLoading();
            axios.post('/api/BankList/GetSysBankList')
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({SysBankList: result.data});
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));
        }

        onSiteChanged(e) {
            let siteId=parseInt(e.target.value);
            let SelSite=null;
            for(let i=0;i< this.state.SiteList.length;i++) {
                let site=this.state.SiteList[i];
                if(site.siteId==siteId) {
                    SelSite=site;
                    break;
                }
            }

            this.setState({SiteId:siteId,SelSite:SelSite}, function(){
                this.onSearch();
            }.bind(this));
        }

        onSearch() {
            //Loading.OnLoading();
            let data = {SiteId: this.state.SiteId};

            /*if(this.state.selectCurrency!=""){
                data.CurrencyID = parseInt(this.state.selectCurrency);
            }*/

            axios.post('/api/BankList/GetLoaclBankList', data)
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({searchData: result, GetData: true});
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));
        }

        showForm(selCcyId){
            this.setState({ShowForm: true, SelCcyId: selCcyId});
        }

        closeForm(){
            this.setState({ShowForm: false});
        }

        onSaveForm() {
            this.onSearch();            
            this.setState({ShowForm: false});
        }

        componentDidMount() {
            this.getSiteList();
            this.getSysBankList();
        }

        onCheckDetail(id) {
            this.setState({transId: id, ShowDetial: true});
        }

        onBack() {
            this.setState({ShowDetial: false});
            this.onRefresh();
        }

        render() {
            let searchData = this.state.searchData;
            let ccyPanel=[];
            if(searchData!=null) {
                let dataContainter = {};
                for(let i=0;i< searchData.data.length;i++){
                    let ccy = dataContainter[searchData.data[i].currencyId]||[];
                    ccy.push(searchData.data[i]);
                    dataContainter[searchData.data[i].currencyId]=ccy;
                }

                for (var prop in dataContainter) {
                    ccyPanel.push(<CcyPanel showForm={this.showForm.bind(this)} key={prop} searchData={dataContainter[prop]} currencyId={prop} onRefresh={this.onRefresh.bind(this)}/>);
                }
            }

            //<CcyPanel searchData={searchData} currencyId onRefresh={this.onRefresh.bind(this)}/>
            let siteList = this.state.SiteList.map(function(obj) { 
                return <div key={obj.siteId} className="custom-control custom-radio custom-control-inline">
                            <input type="radio" id={"SiteId_"+obj.siteId} name="SiteList" className="custom-control-input" checked={this.state.SiteId === obj.siteId} value={obj.siteId} onChange={this.onSiteChanged.bind(this)}/>
                            <label className="custom-control-label" htmlFor={"SiteId_"+obj.siteId}>{obj.siteName}</label>
                        </div>;
            }.bind(this));
            return (
                this.state.ShowForm ? <BankCardForm onSaveForm={this.onSaveForm.bind(this)} closeForm={this.closeForm.bind(this)} {...this.state} onSiteChanged={this.onSiteChanged.bind(this)}/> : 
                <div>
                    {this.state.SysBankList != null ? <button className="btn btn-add" onClick={this.showForm.bind(this, null)}>Add Bank Card</button>:null}
                    <div className="form-group">
                        <label className="col-form-label d-block">Site Name</label>
                        {siteList}
                    </div>
                    {searchData&&searchData.data!=null?ccyPanel:null}
                </div>
            )
        }
    }
    
    ReactDOM.render(<BankCardInfoPanel/>, document.getElementById('BankCardInfoPanel'));
    })(jQuery, React, ReactDOM, i18n, axios, moment);