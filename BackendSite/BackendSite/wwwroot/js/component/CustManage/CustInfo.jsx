import {number_format, getUrlParameter} from '../commJS/common';
import {Pagination} from '../commJS/Pagination.jsx';
import {TpTransferInfo} from './CustInfo_TpTransferInfo.jsx';
import {WithdrawalInfo} from './CustInfo_WithdrawalInfo.jsx';
import {LoginLog } from './CustInfo_LoginLog.jsx';
import {Notes} from './CustInfo_Notes.jsx';
import {ResetPwd} from './CustInfo_ResetPwd.jsx';

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
    
    const DepositStatus = {
        Pending:1,
        Paid :2,
        Approved:3,
        Rejected:4,
        Canceled:5,
        Expired:6,
        OnHold :7
    };

    const DepositStatusOp = {
        1:<span className="status status-pending">{i18n["lbl_DepositStatus1"]}</span>,
        2:<span className="status status-">Paid</span>,
        3:<span className="status status-approved">{i18n["lbl_DepositStatus3"]}</span>,
        4:<span className="status status-rejected">{i18n["lbl_DepositStatus4"]}</span>,
        5:<span className="status status-cancelled">{i18n["lbl_DepositStatus5"]}</span>,
        6:<span className="status status-">Expired</span>,
        7:<span className="status status-hold">{i18n["lbl_DepositStatus7"]}</span>,
    } 

    class DepositList extends React.Component {
        constructor(props) {
            super(props);

        }

        onCheckDetail(id) {
            this.props.onCheckDetail(id);
        }

        onRefresh() {
            this.props.ChangePage(null);
        }

        render() {
            let rows = this.props.searchData.data.map(row => 
                <tr key={row.transId}>
                    <td>{moment(row.createTime).format('l LTS')}</td>                
                    <th scope="row">{row.transId}</th>
                    <td>{i18n["lbl_DepositType" + row.depositType]}</td>
                    <td className="text-right">{number_format(row.amount)}</td>                    
                    <td>{row.accountNo}</td>
                    <td>{row.accountName}</td>
                    <td>{DepositStatusOp[row.status]}</td>
                    <td>{row.refId}</td>
                    <td>{moment(row.depositTime).format('l LTS')}</td>
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
                                <th scope="col">CreateTime {moment().format('(Z)')}</th>                            
                                <th scope="col">Trxn.ID</th>
                                <th scope="col">Deposit Type</th>
                                <th scope="col" className="text-right">Amount</th>
                                <th scope="col">To Acc. NO.</th>
                                <th scope="col">To Acc. Name</th>                                
                                <th scope="col">Status</th>
                                <th scope="col">RefId</th>
                                <th scope="col">DepositTime {moment().format('(Z)')}</th>
                            </tr>
                        </thead>
                        <tbody>
                            {rows}
                        </tbody>
                    </table>
                    <Pagination Data={{...this.props.searchData}} ChangePage={this.props.ChangePage}/>
                </div>
            )
        }
    }

    class DepositInfo extends React.Component {
        constructor(props) {
            super(props);
            this.state = {};

            this.startDate = React.createRef();
            this.endDate = React.createRef();
        }

        componentDidMount() {
            $(this.startDate.current).datepicker({
                format: 'mm/dd/yyyy', autoclose:true
            }); 

            $(this.endDate.current).datepicker({
                format: 'mm/dd/yyyy', autoclose:true
            });
        }

        onStartDate(){
            $(this.startDate.current).focus();
        }

        onEndDate(){
            $(this.endDate.current).focus();
        }        

        handleSubmit(event) {
            event.preventDefault();
            this.props.onChangeValue("DepositList", {StartDate: moment(this.startDate.current.value).unix(), EndDate: moment(this.endDate.current.value).add(1, 'days').unix()-1});
            this.props.GetDepositListByCust(1);
        }

        render() {
            let depositInfo = this.props.DepositList;
            let searchData = this.props.searchData;
            return (
                <div>
                    <form className="filter" onSubmit={this.handleSubmit.bind(this)}>
                        <div className="input-item">
                            <span className="label">From Date</span>
                            <div className="date">
                                <input type="text" ref={this.startDate} name="StartDate" className="dateselect" required="required" data-date-end-date="0d" autoComplete="off" value={depositInfo&&depositInfo.StartDate ? moment.unix(depositInfo.StartDate).format("MM/DD/YYYY"):""} />
                                <i className="icon-calendar dateselect" onClick={this.onStartDate.bind(this)} />
                            </div>                            
                        </div>     
                        <div className="input-item date">
                            <span className="label">To Date</span>
                            <div className="date">
                                <input type="text" ref={this.endDate} name="EndDate" required="required" data-date-end-date="0d" autoComplete="off" value={depositInfo&&depositInfo.EndDate ? moment.unix(depositInfo.EndDate).format("MM/DD/YYYY"):""}/>
                                <i className="icon-calendar dateselect" onClick={this.onEndDate.bind(this)} />
                            </div>                            
                        </div>
                        <div className="button-group"><button className="btn btn-search" type="submit"><span>Search</span></button></div>
                    </form>
                    {searchData!=null ? <DepositList searchData={searchData} ChangePage={this.props.GetDepositListByCust} /> : null}
                </div>
            )
        }
    }

    class FinancialLogList extends React.Component {
        constructor(props) {
            super(props);

        }

        onCheckDetail(id) {
            this.props.onCheckDetail(id);
        }

        onRefresh() {
            this.props.ChangePage(null);
        }

        ChangePage(pageNumber) {
            this.props.ChangePage(pageNumber);
        }

        render() {
            let rows = this.props.searchData.data.map(row => 
                <tr key={row.id}>
                    <td>{moment(row.createTime).format('l LTS')}</td>
                    <td>{row.typeName}</td>
                    <td>{row.direction}</td>
                    <td className="text-right">{number_format(row.changeAmount)}</td>
                    <td className="text-right">{number_format(row.beforeAmount)}</td>                    
                    <td className="text-right">{number_format(row.afterAmount)}</td>
                    <td>{row.memo}</td>                    
                    <td>{row.refTransId}</td>
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
                                <th scope="col">Time {moment().format('(Z)')}</th>
                                <th scope="col">Type</th>
                                <th scope="col">direction</th>
                                <th scope="col" className="text-right">Change Amount</th>
                                <th scope="col" className="text-right">Before Amount</th>
                                <th scope="col" className="text-right">After Amount</th>
                                <th scope="col">Memo</th>
                                <th scope="col">Ref TransId</th>                                
                            </tr>
                        </thead>
                        <tbody>
                            {rows}
                        </tbody>
                    </table>
                    <Pagination Data={{...this.props.searchData}} ChangePage={this.props.ChangePage}/>
                </div>
            )
        }
    }

    class FinancialLog extends React.Component {
        constructor(props) {
            super(props);
            this.state = {};

            this.startDate = React.createRef();
            this.endDate = React.createRef();
        }

        componentDidMount() {
            $(this.startDate.current).datepicker({
                format: 'mm/dd/yyyy', autoclose:true
            }); 

            $(this.endDate.current).datepicker({
                format: 'mm/dd/yyyy', autoclose:true
            });
        }

        onStartDate(){
            $(this.startDate.current).focus();
        }

        onEndDate(){
            $(this.endDate.current).focus();
        }        

        /*onChange(e) {
            this.props.onChange(e, "FinancialLog");
        }*/

        handleSubmit(event) {
            event.preventDefault();
            this.props.onChangeValue("FinancialLog", {StartDate: moment(this.startDate.current.value).unix(), EndDate: moment(this.endDate.current.value).add(1, 'days').unix()-1});
            this.props.GetFinancialLog(1);
        }

        render() {
            let financialLog = this.props.FinancialLog;
            let searchData = this.props.searchData;
            return (
                <div>
                    <form className="filter" onSubmit={this.handleSubmit.bind(this)}>
                        <div className="input-item">
                            <span className="label">From Date</span>
                            <div className="date">
                                <input type="text" ref={this.startDate} name="StartDate" className="dateselect" required="required" data-date-end-date="0d" autoComplete="off" value={financialLog&&financialLog.StartDate ? moment.unix(financialLog.StartDate).format("MM/DD/YYYY"):""} />
                                <i className="icon-calendar dateselect" onClick={this.onStartDate.bind(this)} />
                            </div>                            
                        </div>     
                        <div className="input-item date">
                            <span className="label">To Date</span>
                            <div className="date">
                                <input type="text" ref={this.endDate} name="EndDate" required="required" data-date-end-date="0d" autoComplete="off" value={financialLog&&financialLog.EndDate ? moment.unix(financialLog.EndDate).format("MM/DD/YYYY"):""}/>
                                <i className="icon-calendar dateselect" onClick={this.onEndDate.bind(this)} />
                            </div>                            
                        </div>
                        <div className="button-group"><button className="btn btn-search" type="submit"><span>Search</span></button></div>
                    </form>
                    {searchData!=null ? <FinancialLogList searchData={searchData} ChangePage={this.props.GetFinancialLog} /> : null}
                </div>
            )
        }
    }

    class CustInfo extends React.Component {
        constructor(props) {
            super(props);
            this.state = {};
        }

        componentDidMount() {
            let _clipboard = new ClipboardJS(".js-copy");
        }
        
        onKycChange(e) {
            let data = this.props.CustInfoData;
            axios.post('/api/CustManage/SetCustStatusSwitch', {CustId: data.custId, OP:1})
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.props.onGetCustInfoDetail();
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));       
        }

        onRiskChange(e) {
            let data = this.props.CustInfoData;
            axios.post('/api/CustManage/SetCustStatusSwitch', {CustId: data.custId, OP:2})
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.props.onGetCustInfoDetail();
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));            
        }

        onAskReupload() {
            if(confirm('Are you sure?')) {
                let data = this.props.CustInfoData;
                axios.post('/api/CustManage/DelCustPhotoId', {CustId: data.custId})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.props.onGetCustInfoDetail();
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
            let data = this.props.CustInfoData;
            let tpWallet= null;
            if(data!=null && data.tpWallet!=null) {
                tpWallet = <ul className="wallet-wrap">{data.tpWallet.map(function(wallet) { 
                    return <li key={wallet.tpId} className="wallet-item table-detail">
                                <span>{wallet.tpName}</span>
                                <table><tbody><tr><td><span>{wallet.tpUserName}</span></td><td><button type="button" className="btn btn-copy js-copy" data-clipboard-text={wallet.tpUserName}>Copy</button></td></tr></tbody></table>
                                <span className={wallet.balance <0 ? "negative-num":"positive-num"}>{number_format(wallet.balance)}</span>
                            </li>;
                }.bind(this))}</ul>;
            }

            return (
                data==null ? <div className="text-center">
                    <div className="spinner-border" role="status">
                        <span className="sr-only">Loading...</span>
                    </div>
                </div> : 
                <div>
                    <div className="flex-wrap">
                        <table className="table table-detail">
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
                                    <th scope="row">Full Name</th>
                                    <td>{data.accountName}</td>
                                </tr>
                                <tr>
                                    <th scope="row">Country Code</th>
                                    <td>{data.countryCode}</td>
                                </tr>
                                <tr>
                                    <th scope="row">Phone Number</th>
                                    <td>{"("+data.areaCode+") "+data.phoneNo}</td>
                                </tr>                                               
                                <tr>
                                    <th scope="row">Act Status</th>
                                    <td>{CustActStatusOp[data.actStatus]}</td>
                                </tr>
                                <tr>
                                    <th scope="row">IsTest</th>
                                    <td>
                                        <span>{data.isTest ? "Yes" : "No"}</span>
                                        {/*<div className="switch__container">
                                            <input id="switch-isTest" className="switch switch--flat" type="checkbox"/>
                                            <label htmlFor="switch-isTest"></label>
                                        </div>*/}
                                    </td>
                                </tr>
                                <tr>
                                    <th scope="row">IsKyc</th>
                                    <td>
                                        {data.isKyc ? "Yes" : "No"}
                                        <div className="switch__container">
                                            <input id="switch-isKyc" className="switch switch--flat" checked={data.isKyc} type="checkbox" onChange={this.onKycChange.bind(this)}/>
                                            <label htmlFor="switch-isKyc"></label>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <th scope="row">IsRisk</th>
                                    <td>{data.isRisk ? "Yes" : "No"}
                                        <div className="switch__container">
                                            <input id="switch-isRisk" className="switch switch--flat" checked={data.isRisk} type="checkbox" onChange={this.onRiskChange.bind(this)}/>
                                            <label htmlFor="switch-isRisk"></label>
                                        </div>
                                    </td>
                                </tr>                                                        
                                <tr>
                                    <th scope="row">Status</th>
                                    <td>{CustStatusOp[data.status]}</td>
                                </tr>
                                <tr>
                                    <th scope="row">Registration Time</th>
                                    <td>{moment(data.regTime).format('l LTS (Z)')}</td>
                                </tr>                            
                                <tr>
                                    <th scope="row">Last Login Time</th>
                                    <td>{moment(data.lastLoginTime).format('l LTS (Z)')}</td>
                                </tr>
                                <tr>
                                    <th scope="row">First Deposit Time</th>
                                    <td>{moment(data.firstDepositTime).format('l LTS (Z)')}</td>
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
                        
                        {data.hasPhotoId ? <div className="id-image">
                            <img src={"/CustManage/GetCustPhotoId?CustId=" + data.custId} alt="Identity Card" />
                            <button className="btn btn-delete" disabled={data.isKyc} onClick={!data.isKyc ? this.onAskReupload.bind(this) : null}>Ask Re-Upload</button>
                        </div>:null}
                    </div>
                    {tpWallet}
                </div>
            )
        }
    }
    
    class CustInfoPanel extends React.Component {
        _Tabs=["Customer Info","Financial Log","Deposit Info","Transfer Info","Withdrawal Info","Login Log", "Reset Password", "Notes"];

        constructor(props) {
            super(props);
            this.state = { CustInfoData: null, TabIdx: 0, FinancialLog:{}, Notes:null};
        }

        onTab(idx) {
            this.setState({TabIdx: idx});
        }

        componentDidMount() {
            this.onGetCustInfoDetail();
        }

        GetFinancialLog(PageNumber) {
            let pageNumber = PageNumber||this.state.FinancialLogData.pageNumber;
            let CustId = getUrlParameter("custId");
            if(!isNaN(CustId)) {       
                let iCustId = parseInt(CustId); 
                axios.post('/api/Payment/GetFinancialLog', {...this.state.FinancialLog, CustId: iCustId, PageNumber: pageNumber})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({FinancialLogData: result});
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
            }     
        }

        GetDepositListByCust(PageNumber) {
            let pageNumber = PageNumber||this.state.DepositListData.pageNumber;
            let CustId = getUrlParameter("custId");
            if(!isNaN(CustId)) {       
                let iCustId = parseInt(CustId); 
                axios.post('/api/Deposit/GetDepositListByCust', {...this.state.DepositList, CustId: iCustId, PageNumber: pageNumber})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({DepositListData: result});
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
            }
        }

        GetWithdrawalListByCust(PageNumber) {
            let pageNumber = PageNumber||this.state.WithdrawalListData.pageNumber;
            let CustId = getUrlParameter("custId");                        
            if(!isNaN(CustId)) {       
                let iCustId = parseInt(CustId); 
                axios.post('/api/Withdrawal/GetWithdrawalListByCust', {...this.state.WithdrawalInfo, CustId: iCustId, PageNumber: pageNumber})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({WithdrawalListData: result});
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
            }
        }                 

        GetTpTransferInfoByCust(PageNumber) {
            let pageNumber = PageNumber||this.state.TpTransferInfoData.pageNumber;
            let CustId = getUrlParameter("custId");                        
            if(!isNaN(CustId)) {       
                let iCustId = parseInt(CustId); 
                axios.post('/api/Tp/GetTpTransferInfoByCust', {...this.state.TpTransferInfo, CustId: iCustId, PageNumber: pageNumber})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({TpTransferInfoData: result});
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
            }        
        }

        GetLoginLogByCust(PageNumber) {
            let pageNumber = PageNumber||this.state.LoginLogData.pageNumber;
            let CustId = getUrlParameter("custId");                        
            if(!isNaN(CustId)) {       
                let iCustId = parseInt(CustId); 
                axios.post('/api/CustManage/GetLoginLogByCust', {...this.state.LoginLog, CustId: iCustId, PageNumber: pageNumber})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({LoginLogData: result});
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
            }
        }

        AddCustNote(note) {
            let CustId = getUrlParameter("custId");                        
            if(!isNaN(CustId)) {            
                let iCustId = parseInt(CustId);            
                axios.post('/api/CustManage/AddCustNote', {CustId: iCustId, Note: note})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.GetCustNotes();
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
            }
        }

        GetCustNotes() {
            let CustId = getUrlParameter("custId");            
            if(!isNaN(CustId)) {            
                let iCustId = parseInt(CustId);
                axios.post('/api/CustManage/GetCustNotes', {CustId: iCustId})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({Notes: result.data});
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));            
            }
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
                        this.setState({CustInfoData: result.data}, function(){
                            let _clipboard = new ClipboardJS(".js-copy");
                        });
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
            }
        }
        

        onChange(e, TabName) {
            let Obj = this.state[TabName];
            Obj[event.target.name]=event.target.value;

            let obj=this.state;
            obj[TabName]=Obj;
            this.setState(obj);
        }

        onChangeValue(TabName, Obj) {
            let obj=this.state;
            obj[TabName]=Obj;
            this.setState(obj);
        }        

        render() {
            let dataContainer = null;
            let TabIdx = this.state.TabIdx;

            if(TabIdx == 0){
                dataContainer = <CustInfo CustInfoData={this.state.CustInfoData} onGetCustInfoDetail={this.onGetCustInfoDetail.bind(this)}/>;
            } else if(TabIdx == 1){
                dataContainer = <FinancialLog GetFinancialLog={this.GetFinancialLog.bind(this)} searchData={this.state.FinancialLogData} FinancialLog={this.state.FinancialLog} onChangeValue={this.onChangeValue.bind(this)}/>;
            } else if(TabIdx == 2){
                dataContainer = <DepositInfo  GetDepositListByCust={this.GetDepositListByCust.bind(this)} searchData={this.state.DepositListData} DepositList={this.state.DepositList} onChangeValue={this.onChangeValue.bind(this)} />;
            } else if(TabIdx == 3){
                dataContainer = <TpTransferInfo GetTpTransferInfoByCust={this.GetTpTransferInfoByCust.bind(this)} searchData={this.state.TpTransferInfoData} TpTransferInfo={this.state.TpTransferInfo} onChangeValue={this.onChangeValue.bind(this)} />;
            } else if(TabIdx == 4){
                dataContainer = <WithdrawalInfo GetWithdrawalListByCust={this.GetWithdrawalListByCust.bind(this)} searchData={this.state.WithdrawalListData} WithdrawalInfo={this.state.WithdrawalInfo} onChangeValue={this.onChangeValue.bind(this)} />;
            } else if(TabIdx == 5){
                dataContainer = <LoginLog GetLoginLogByCust={this.GetLoginLogByCust.bind(this)} searchData={this.state.LoginLogData} LoginLog={this.state.LoginLog} onChangeValue={this.onChangeValue.bind(this)} />;
            } else if(TabIdx == 6) {
                dataContainer = <ResetPwd />;
            } else if(TabIdx == 7) {
                dataContainer = <Notes AddCustNote={this.AddCustNote.bind(this)} Notes={this.state.Notes} GetCustNotes={this.GetCustNotes.bind(this)}/>;
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
                                        <span className="name">{getUrlParameter("UserName")}</span>
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