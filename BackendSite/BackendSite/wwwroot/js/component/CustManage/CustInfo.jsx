import { number_format, getUrlParameter } from '../commJS/common';
import { Pagination } from '../commJS/Pagination.jsx';
import { TpTransferInfo } from './CustInfo_TpTransferInfo.jsx';
import { WithdrawalInfo } from './CustInfo_WithdrawalInfo.jsx';
import { LoginLog } from './CustInfo_LoginLog.jsx';
import { Notes } from './CustInfo_Notes.jsx';
import { ResetPwd } from './CustInfo_ResetPwd.jsx';
import * as Loading from '../commJS/Loading.jsx';
import PersonalMsg from './PersonalMsg/CustInfo_PersonalMsg';
import TurnoverReport from './TurnoverReport/index';
import { AdjustmentInfo } from './CustInfo_AdjustmentInfo.jsx';
import { AdminUserLog } from './CustInfo_AdminUserLog.jsx';
import { BankLog } from './CustInfo_BankLog.jsx';
import { CryptoWalletLog } from './CustInfo_CryptoWalletLog.jsx';
import DateTimePicker from 'react-datetime';
import TpBetRecord from './TpBetRecord/index';
import VipLevel from './VipLevel/index';
import PromotionReport from './PromotionReport/index';
import SetCustStatusNote from './SetCustStatusNote/PopboxContentSetNote.js';
import { DepositInfoPagination } from './CustInfo_DepositInfoPagination.jsx';
import { PaginationTotalAmount } from '../commJS/PaginationTotalAmount.jsx';
import Tag from './Tag/index';
import { NewPagination } from '../commJS/NewPagination.jsx';
import WithdrawalLimitLog from './WithdrawalLimitLog/index';

(function ($, React, ReactDOM, i18n, axios, moment) {
    const CustStatusOp = {
        1: <span className="status status-active">Running</span>,
        2: <span className="status status-suspend">Suspend</span>,
        3: <span className="status status-closed">Closed</span>,
        4: <span className="status status-lock">Lock</span>
    }

    const CustActStatusOp = {
        1: <span className="status status-initial">Non-Deposit</span>,
        2: <span className="status status-active">Deposited</span>,
    }

    const CustIovationOp = {
        "A": <span className="status status-active">Allow</span>,
        "R": <span className="status status-suspend">Review</span>,
        "D": <span className="status status-closed">Deny</span>,
    }

    const DepositStatus = {
        Pending: 1,
        Paid: 2,
        Approved: 3,
        Rejected: 4,
        Canceled: 5,
        Expired: 6,
        OnHold: 7
    };

    const DepositStatusOp = {
        1: <span className="status status-pending">{i18n["lbl_DepositStatus1"]}</span>,
        2: <span className="status status-">Paid</span>,
        3: <span className="status status-approved">{i18n["lbl_DepositStatus3"]}</span>,
        4: <span className="status status-rejected">{i18n["lbl_DepositStatus4"]}</span>,
        5: <span className="status status-cancelled">{i18n["lbl_DepositStatus5"]}</span>,
        6: <span className="status status-">Expired</span>,
        7: <span className="status status-hold">{i18n["lbl_DepositStatus7"]}</span>,
    }

    const BonusInfoStatus = {
        1: <span className="status status-active">Running</span>,
        2: <span className="status status-suspend">Canceled</span>,
        3: <span className="status status-closed">Expired</span>,
        4: <span className="status status-initial">Reached</span>,
    }

    const Direction = {
        1: "+",
        2: "-"
    }

    const DepositStatusSelectOp = {
        1: "Pending",
        7: "On Hold",
        3: "Approved",
        4: "Rejected",
        5: "Cancelled"
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
                    <td>{moment.utc(row.createTime).add(8,'hour').format('l LTS')}</td>
                    <th scope="row">{row.transId}</th>
                    <td>{i18n["lbl_DepositType" + row.depositType]}</td>
                    <td className="text-right">{number_format(row.amount)}</td>
                    <td className="text-right">{number_format(row.actualDeposit)}</td>
                    <td>{row.bankName}</td>
                    <td>{row.accountNo}</td>
                    <td>{row.accountName}</td>
                    <td>{DepositStatusOp[row.status]}</td>
                    <td>{row.refId}</td>
                    <td>{moment.utc(row.depositTime).add(8,'hour').format('l LTS')}</td>
                    <td>{moment.utc(row.updateTime).add(8,'hour').format('l LTS')}</td>
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
                                <th scope="col">CreateTime (Adjusted to GMT+8)</th>
                                <th scope="col">Trxn.ID</th>
                                <th scope="col">Deposit Type</th>
                                <th scope="col" className="text-right">Amount</th>
                                <th scope="col" className="text-right">Actual Deposit</th>
                                <th scope="col">Bank Name</th>
                                <th scope="col">To Acc. NO.</th>
                                <th scope="col">To Acc. Name</th>
                                <th scope="col">Status</th>
                                <th scope="col">RefId</th>
                                <th scope="col">DepositTime (Adjusted to GMT+8)</th>
                                <th scope="col">UpdateTime (Adjusted to GMT+8)</th>
                            </tr>
                        </thead>
                        <tbody>
                            {rows}
                        </tbody>
                    </table>
                    <DepositInfoPagination Data={{ ...this.props.searchData }} ChangePage={this.props.ChangePage} />
                </div>
            )
        }
    }

    class DepositInfo extends React.Component {
        constructor(props) {
            super(props);
            this.state = { DepositStatus: 0 };
            this.startDate = React.createRef();
            this.endDate = React.createRef();
        }

        componentDidMount() {
            $(this.startDate.current).datepicker({
                format: 'mm/dd/yyyy', autoclose: true
            });

            $(this.endDate.current).datepicker({
                format: 'mm/dd/yyyy', autoclose: true
            });
        }

        onStartDate() {
            $(this.startDate.current).focus();
        }

        onEndDate() {
            $(this.endDate.current).focus();
        }

        onChange(event) {
            let obj = this.state;
            obj[event.target.name] = event.target.value;
            this.setState(obj);
        }

        handleSubmit(event) {
            event.preventDefault();
            this.props.onChangeValue("DepositList", { StartDate: moment(this.startDate.current.value).unix(),
                 EndDate: moment(this.endDate.current.value).add(1, 'days').unix() - 1,
                 Status: parseInt(this.state.DepositStatus),
                 UTCStartDate:moment.utc(this.startDate.current.value).unix(),
                 UTCEndDate:moment.utc(this.endDate.current.value).add(1, 'days').unix()-1 });
            this.props.GetDepositListByCust(1);
        }

        render() {
            let depositInfo = this.props.DepositList;
            let searchData = this.props.searchData;
            let depositStatusOption = null;
            if (DepositStatusSelectOp) {
                depositStatusOption = Object.keys(DepositStatusSelectOp).map(i => {
                    let selected = this.state.DepositStatus === i ? "selected" : "";
                    return (
                        <option key={`Deposit-${i}`} value={i} selected={selected}>{DepositStatusSelectOp[i]}</option>
                    )
                })
            }
            return (
                <div>
                    <form className="filter" onSubmit={this.handleSubmit.bind(this)}>
                        <div className="input-item">
                            <span className="label">From Date</span>
                            <div className="date">
                                <input type="text" ref={this.startDate} name="StartDate" className="dateselect" required="required" data-date-end-date="0d" autoComplete="off" value={depositInfo && depositInfo.StartDate ? moment.unix(depositInfo.StartDate).format("MM/DD/YYYY") : ""} />
                                <i className="icon-calendar dateselect" onClick={this.onStartDate.bind(this)} />
                            </div>
                        </div>
                        <div className="input-item date">
                            <span className="label">To Date</span>
                            <div className="date">
                                <input type="text" ref={this.endDate} name="EndDate" required="required" data-date-end-date="0d" autoComplete="off" value={depositInfo && depositInfo.EndDate ? moment.unix(depositInfo.EndDate).format("MM/DD/YYYY") : ""} />
                                <i className="icon-calendar dateselect" onClick={this.onEndDate.bind(this)} />
                            </div>
                        </div>
                        <div className="input-item">
                            <span className="label">Deposit Status</span>
                            <div className="">
                                <select onChange={this.onChange.bind(this)} name="DepositStatus">
                                    <option value="0" selected="selected">All</option>
                                    {depositStatusOption}
                                </select>
                            </div>
                        </div>
                        <div className="button-group"><button className="btn btn-search" type="submit"><span>Search</span></button></div>
                        <div>(Adjusted to GMT+8)</div>
                    </form>
                    {searchData != null ? <DepositList searchData={searchData} ChangePage={this.props.GetDepositListByCust} /> : null}
                </div>
            )
        }
    }

    class BonusLogList extends React.Component {
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
                    <td>{moment.utc(row.createTime).add(8,'hour').format('l LTS')}</td>
                    <td>{row.typeName ? row.typeName : "N/A"}</td>
                    <td className="text-right">{Direction[row.direction] + number_format(row.changeBonus)}</td>
                    <td className="text-right">{number_format(row.beforeBonus)}</td>
                    <td className="text-right">{number_format(row.afterBonus)}</td>
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
                                <th scope="col">Time (Adjusted to GMT+8)</th>
                                <th scope="col">Type</th>
                                <th scope="col" className="text-right">Change Bonus</th>
                                <th scope="col" className="text-right">Before Bonus</th>
                                <th scope="col" className="text-right">After Bonus</th>
                                <th scope="col">Memo</th>
                                <th scope="col">Ref TransId</th>
                            </tr>
                        </thead>
                        <tbody>
                            {rows}
                        </tbody>
                    </table>
                    <NewPagination Data={{ ...this.props.searchData }} ChangePage={this.props.ChangePage} />
                </div>
            )
        }
    }

    class BonusLog extends React.Component {
        constructor(props) {
            super(props);
            this.state = { RefTransId: "", PromotionCategory: "" };

            this.startDate = React.createRef();
            this.endDate = React.createRef();

        }

        componentDidMount() {
            $(this.startDate.current).datepicker({
                format: 'mm/dd/yyyy', autoclose: true
            });

            $(this.endDate.current).datepicker({
                format: 'mm/dd/yyyy', autoclose: true
            });
            this.props.GetPromotionCategory()
        }

        onStartDate() {
            $(this.startDate.current).focus();
        }

        onEndDate() {
            $(this.endDate.current).focus();
        }

        onChange(event) {
            let obj = this.state;
            obj[event.target.name] = event.target.value;
            this.setState(obj);
        }

        handleSubmit(event) {
            event.preventDefault();
            this.props.onChangeValue("BonusLog", { StartDate: moment(this.startDate.current.value).unix(), EndDate: moment(this.endDate.current.value).add(1, 'days').unix() - 1, RefTransId: this.state.RefTransId.trim() == '' ? null : this.state.RefTransId, PromoCategoryId: this.state.PromotionCategory == '' ? null : parseInt(this.state.PromotionCategory),
                                    UTCStartDate:moment.utc(this.startDate.current.value).unix(),UTCEndDate:moment.utc(this.endDate.current.value).add(1, 'days').unix()-1 });
            this.props.GetBonusLog(1);
        }

        componentDidUpdate(prevProps) {
            if (this.props.PromotionCategoryData !== prevProps.PromotionCategoryData) {
                this.setState({ PromotionCategory: this.props.PromotionCategoryData[0].id });
            }
        }

        render() {
            let bonusLog = this.props.BonusLog;
            let searchData = this.props.searchData;
            let promotionCategoryData = this.props.PromotionCategoryData;
            let promotionCategoryOption = null;

            if (promotionCategoryData) {
                promotionCategoryOption = promotionCategoryData.map(i => {
                    let selected = this.state.PromotionCategory == i.id ? "selected" : "";
                    return (
                        <option key={i.id} value={i.id} selected={selected}>{i.name}</option>
                    )
                })
            }

            return (
                <div>
                    <form className="filter" onSubmit={this.handleSubmit.bind(this)}>
                        <div className="input-item">
                            <span className="label">Promotion Category</span>
                            <div className="">
                                <select onChange={this.onChange.bind(this)} name="PromotionCategory">
                                    {promotionCategoryOption}
                                </select>
                            </div>
                        </div>
                        <div className="input-item">
                            <span className="label">From Date</span>
                            <div className="date">
                                <input type="text" ref={this.startDate} name="StartDate" className="dateselect" data-date-end-date="0d" autoComplete="off" value={bonusLog && bonusLog.StartDate ? moment.unix(bonusLog.StartDate).format("MM/DD/YYYY") : ""} />
                                <i className="icon-calendar dateselect" onClick={this.onStartDate.bind(this)} />
                            </div>
                        </div>
                        <div className="input-item date">
                            <span className="label">To Date</span>
                            <div className="date">
                                <input type="text" ref={this.endDate} name="EndDate" data-date-end-date="0d" autoComplete="off" value={bonusLog && bonusLog.EndDate ? moment.unix(bonusLog.EndDate).format("MM/DD/YYYY") : ""} />
                                <i className="icon-calendar dateselect" onClick={this.onEndDate.bind(this)} />
                            </div>
                        </div>
                        <div className="input-item">
                            <span className="label">Ref TransId</span>
                            <div className="">
                                <input type="text" onChange={this.onChange.bind(this)} placeholder="Ref TransId" name="RefTransId" autoComplete="off" value={this.state.RefTransId} />
                            </div>
                        </div>
                        <div className="button-group"><button className="btn btn-search" type="submit"><span>Search</span></button></div>
                        <div>(Adjusted to GMT+8)</div>
                    </form>
                    {searchData != null ? <BonusLogList searchData={searchData} ChangePage={this.props.GetBonusLog} /> : null}
                </div>
            )
        }
    }

    class RebateLogList extends React.Component {
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

        // render() {
        //     let rows = this.props.searchData.data.map(row =>
        //         <tr key={row.id}>
        //             <td>{moment(row.createTime).format('l LTS')}</td>
        //             <td>{row.gameName ? row.gameName : "N/A"}</td>
        //             <td>{row.rebateTypeName ? row.rebateTypeName : "N/A"}</td>
        //             <td className="text-right">{number_format(row.redeemAmount)}</td>
        //             <td>{row.bonusCode ? row.bonusCode : "N/A"}</td>
        //         </tr>
        //     );

        //     return (
        //         <div className="table">
        //             <div className="table-options">
        //                 <button type="button" className="btn btn-refresh" onClick={this.onRefresh.bind(this)}>
        //                     Refresh
        //                 </button>
        //             </div>
        //             <table className="table table-striped">
        //                 <thead>
        //                     <tr>
        //                         <th scope="col">Timexxxx {moment().format('(Z)')}</th>
        //                         <th scope="col">Game Name</th>
        //                         <th scope="col">Rebate Type Name</th>
        //                         <th scope="col" className="text-right">Redeem Amount</th>
        //                         <th scope="col">Bonus Code</th>
        //                     </tr>
        //                 </thead>
        //                 <tbody>
        //                     {rows}
        //                 </tbody>
        //             </table>
        //             <Pagination Data={{ ...this.props.searchData }} ChangePage={this.props.ChangePage} />
        //         </div>
        //     )
        // }

   
        render() {
            let rows = this.props.searchData.data.map(row =>
                <tr key={row.id}>
                    <td>{moment.utc(row.createTime).add(8,'hour').format('l LTS')}</td>
                    <td>{row.categoryName }</td>
                    <td className="text-right">{number_format(row.rcRedeemAmount)}</td>
                    <td>{row.refId}</td>
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
                            <th scope="col">Time (Adjusted to GMT+8)</th>  
                            <th scope="col">Category</th>
                            <th scope="col" className="text-right">Redeem Amount</th>
                            <th scope="col">RefId</th>                            
                            </tr>
                        </thead>
                        <tbody>
                            {rows}
                        </tbody>
                    </table>
                    <PaginationTotalAmount Data={{ ...this.props.searchData }} ChangePage={this.props.ChangePage} />
                </div>
            )
        }
    }

    class ReferralHistoryList extends React.Component {
        constructor(props) {
            super(props);

        }

        ChangePage(pageNumber) {
            this.props.ChangePage(pageNumber);
        }
    
        render() {
            let rows = this.props.searchData.data.map((row, idx) =>
                <tr key={idx}>             
                    <th>{row.userName}</th>
                    <th>{row.currency}</th>
                    <th>{moment.utc(row.registerDate).add(8,'hour').format('l LTS')}</th>
                    {/* <th>{row.registerDate}</th> */}
                    <th>{row.totalDeposit}/{row.depositC}</th>
                    <th>{row.totalBet}/{row.vaildBetC}</th>
                    <th>{row.bonusAmount}</th>
                    {/* <th>{row.rewardTime}</th> */}
                    <th>{row.rewardTime ? moment.utc(row.rewardTime).add(8,'hour').format('l LTS') : null}</th>
                    <th>{row.statusName}</th>
                </tr>
            );

            return (
                <div className="table">
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                <th scope="col">UserName</th>
                                <th scope="col">Currency</th>
                                <th scope="col">Reg Time (Adjusted to GMT+8)</th>
                                <th scope="col">TotalDeposit</th>
                                <th scope="col">TotalBet</th>
                                <th scope="col">Reward Amount</th>
                                <th scope="col">Reward Time (Adjusted to GMT+8)</th>
                                <th scope="col">Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            {rows}
                        </tbody>
                    </table>
                    <NewPagination Data={{ ...this.props.searchData }} ChangePage={this.props.ChangePage} />
                </div>
            )

        }
    }

    class RebateLog extends React.Component {
        constructor(props) {
            super(props);
            this.state = {};

            this.startDate = React.createRef();
            this.endDate = React.createRef();

        }

        componentDidMount() {
            $(this.startDate.current).datepicker({
                format: 'mm/dd/yyyy', autoclose: true
            });

            $(this.endDate.current).datepicker({
                format: 'mm/dd/yyyy', autoclose: true
            });
        }

        onStartDate() {
            $(this.startDate.current).focus();
        }

        onEndDate() {
            $(this.endDate.current).focus();
        }

        onChange(event) {
            let obj = this.state;
            obj[event.target.name] = event.target.value;
            this.setState(obj);
        }

        handleSubmit(event) {
            event.preventDefault();
            this.props.onChangeValue("RebateLog", { StartDate: moment(this.startDate.current.value).unix(), EndDate: moment(this.endDate.current.value).add(1, 'days').unix() - 1,
                                    UTCStartDate:moment.utc(this.startDate.current.value).unix(),UTCEndDate:moment.utc(this.endDate.current.value).add(1, 'days').unix()-1 });
            this.props.GetRebateLog(1);
        }

        componentDidUpdate(prevProps) {
            if (this.props.PromotionCategoryData !== prevProps.PromotionCategoryData) {
                this.setState({ PromotionCategory: this.props.PromotionCategoryData[0].id });
            }
        }

        render() {
            let rebateLog = this.props.RebateLog;
            let searchData = this.props.searchData;
            return (
                <div>
                    <form className="filter" onSubmit={this.handleSubmit.bind(this)}>
                        <div className="input-item">
                            <span className="label">From Date</span>
                            <div className="date">
                                <input type="text" ref={this.startDate} name="StartDate" className="dateselect" data-date-end-date="0d" autoComplete="off" value={rebateLog && rebateLog.StartDate ? moment.unix(rebateLog.StartDate).format("MM/DD/YYYY") : ""} />
                                <i className="icon-calendar dateselect" onClick={this.onStartDate.bind(this)} />
                            </div>
                        </div>
                        <div className="input-item date">
                            <span className="label">To Date</span>
                            <div className="date">
                                <input type="text" ref={this.endDate} name="EndDate" data-date-end-date="0d" autoComplete="off" value={rebateLog && rebateLog.EndDate ? moment.unix(rebateLog.EndDate).format("MM/DD/YYYY") : ""} />
                                <i className="icon-calendar dateselect" onClick={this.onEndDate.bind(this)} />
                            </div>
                        </div>
                        <div className="button-group"><button className="btn btn-search" type="submit"><span>Search</span></button></div>
                        <div>(Adjusted to GMT+8)</div>
                    </form>
                    {searchData != null ? <RebateLogList searchData={searchData} ChangePage={this.props.GetRebateLog} /> : null}
                </div>
            )
        }
    }

    class ReferralHistory extends React.Component {
        constructor(props) {
            super(props);
            this.state = {};
            this.props.GetReferralHistory(1);
        }

        onChange(event) {
            let obj = this.state;
            obj[event.target.name] = event.target.value;
            this.setState(obj);
        }

        render() {
            let searchData = this.props.searchData;
            return (
                <div>
                    {searchData != null ? <ReferralHistoryList searchData={searchData} ChangePage={this.props.GetReferralHistory} /> : null}
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
                    <td>{moment.utc(row.createTime).add(8,'hour').format('l LTS')}</td>
                    <td>{row.typeName}</td>
                    <td className="text-right">{Direction[row.direction] + number_format(row.changeAmount)}</td>
                    <td className="text-right">{number_format(row.beforeAmount)}</td>
                    <td className="text-right">{number_format(row.afterAmount)}</td>
                    <td title={row.memo}>{row.memo}</td>
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
                                <th scope="col">Time (Adjusted to GMT+8)</th>
                                <th scope="col">Type</th>
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
                    <NewPagination Data={{ ...this.props.searchData }} ChangePage={this.props.ChangePage} />
                </div>
            )
        }
    }

    class FinancialLog extends React.Component {
        constructor(props) {
            super(props);
            this.state = { RefTransId: "" };

            this.startDate = React.createRef();
            this.endDate = React.createRef();
        }

        componentDidMount() {
            // $(this.startDate.current).datepicker({
            //     format: 'mm/dd/yyyy', autoclose: true
            // });

            // $(this.endDate.current).datepicker({
            //     format: 'mm/dd/yyyy', autoclose: true
            // });
        }

        onStartDate() {
            // $(this.startDate.current).focus();
        }

        onEndDate() {
            // $(this.endDate.current).focus();
        }

        onChange(event) {
            let obj = this.state;
            obj[event.target.name] = event.target.value;
            this.setState(obj);
        }

        handleSubmit(event) {
            event.preventDefault();
            this.props.onChangeValue("FinancialLog", { 
                StartDate: moment(this.startDate.current.state.inputValue).unix(),
                EndDate: moment(this.endDate.current.state.inputValue).unix(), 
                RefTransId: this.state.RefTransId.trim() == '' ? null : this.state.RefTransId,
                UTCStartDate:moment.utc(this.startDate.current.state.inputValue).unix(),
                UTCEndDate:moment.utc(this.endDate.current.state.inputValue).unix()-1 });
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
                            <DateTimePicker ref={this.startDate} defaultValue={financialLog && financialLog.StartDate ? moment.unix(financialLog.StartDate).format("l LTS") : ""} />
                        </div>
                        <div className="input-item">
                            <span className="label">To Date</span>
                            <DateTimePicker ref={this.endDate} defaultValue={financialLog && financialLog.EndDate ? moment.unix(financialLog.EndDate).format("l LTS") : ""} />
                        </div>
                        <div className="input-item">
                            <span className="label">Ref TransId</span>
                            <div className="">
                                <input type="text" onChange={this.onChange.bind(this)} placeholder="Ref TransId" name="RefTransId" autoComplete="off" value={this.state.RefTransId} />
                            </div>
                        </div>
                        <div className="button-group"><button className="btn btn-search" type="submit"><span>Search</span></button></div>
                        <div>(Adjusted to GMT+8)</div>
                    </form>
                    {searchData != null ? <FinancialLogList searchData={searchData} ChangePage={this.props.GetFinancialLog} /> : null}
                </div>
            )
        }
    }

    class PromotionRow extends React.Component {
        CancelBonus(BonusId) {
            this.props.CancelBonus(BonusId)
        }

        render() {
            let data = this.props.Promotion;
            return (
                <tr>
                    <td>{data.bonusCode}</td>
                    <td>{data.description}</td>
                    <td>{number_format(data.bonusAmount)}</td>
                    <td>{number_format(data.lockedAmount)}</td>
                    <td>{number_format(data.custRollOver.rollOver)}</td>
                    <td>{number_format(data.custRollOver.turnOver)}</td>
                    <td>{BonusInfoStatus[data.bonusInfoStatus]}</td>
                    <td>{data.bonusInfoStatus == 1 ? <button className="btn btn-primary" onClick={this.CancelBonus.bind(this, data.bonusId)}>Cancel Bonus</button> : null}</td>
                </tr>
            )
        }
    }

    class Promotion extends React.Component {

        render() {
            // let Promotions = this.props.Promotions;
            // let promotionRows = null;

            // if (Promotions != null) {
            //     promotionRows = <tbody>{Promotions.map(x => <PromotionRow key={x.bonusCode} Promotion={x} CancelBonus={this.props.CancelBonus} />)}</tbody>;
            // }

            // return (
            //     <div className="table-responsive-md table">
            //         <span class="label">Promotion</span>
            //         <table className="table table-striped">
            //             <thead>
            //                 <tr>
            //                     <th scope="col">BonusCode</th>
            //                     <th scope="col">Description</th>
            //                     <th scope="col">Bonus Amount</th>
            //                     <th scope="col">Locked Amount</th>
            //                     <th scope="col">Rollover</th>
            //                     <th scope="col">Turnover</th>
            //                     <th scope="col">Status</th>
            //                     <th scope="col"></th>
            //                 </tr>
            //             </thead>
            //             {promotionRows}
            //         </table>
            //     </div>
            // )
            return(<></>)
        }
    }

    class BonusWallet extends React.Component {

        render() {
            let BonusWalletRows = this.props.BonusWallet != null ?
                <tbody>
                    {
                        this.props.BonusWallet.map(x => {
                            return (
                                <tr>
                                    <td>{x.promotionCategory}</td>
                                    <td>{x.bonusCode}</td>
                                    <td>{number_format(x.bonus)}</td>
                                </tr>
                            )
                        }
                        )
                    }
                </tbody>
                :
                null;
            return (
                <div className="table-responsive-md table">
                    <span class="label">Bonus Wallet</span>
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                <th scope="col">Promotion Category</th>
                                <th scope="col">Bonus Code</th>
                                <th scope="col">Bonus</th>
                            </tr>
                        </thead>
                        {BonusWalletRows != null ? BonusWalletRows : null}
                    </table>
                </div>
            )
        }
    }

    class RebateInfo extends React.Component {
        render() {
            let RebateInfoRows = this.props.RebateInfo != null ?
                <tbody>
                    {
                        this.props.RebateInfo.map(x => {
                            return (
                                <tr>
                                    <td>{x.bonusCode}</td>
                                    <td>{x.description}</td>
                                    <td>{x.rebateTypeName}</td>
                                    <td>{moment(x.claimTime).format('l LTS')}</td>
                                </tr>
                            )
                        }
                        )
                    }
                </tbody>
                :
                null;
            return (
                <div className="table-responsive-md table">
                    <span class="label">Rebate Info</span>
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                <th scope="col">Bonus Code</th>
                                <th scope="col">Description</th>
                                <th scope="col">Rebate Type Name</th>
                                <th scope="col">Claim Time</th>
                            </tr>
                        </thead>
                        {RebateInfoRows != null ? RebateInfoRows : null}
                    </table>
                </div>
            )
        }
    }

    class CustInfo extends React.Component {
        constructor(props) {
            super(props);
            this.state = {isOpen:false};
        }
        EditFiled = (v, datacolumn) => { this.setState({ ...this.state, [datacolumn]: v }); };

        componentDidMount() {
            let _clipboard = new ClipboardJS(".js-copy");
        }

        onKycChange(e) {
            if (confirm('Are you sure change IsKyc ?')) {
                let data = this.props.CustInfoData;
                axios.post('/api/CustManage/SetCustStatusSwitch', { CustId: data.custId, OP: 1 })
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

        onRiskChange(e) {
            if (confirm('Are you sure change IsRisk ?')) {
                let data = this.props.CustInfoData;
                axios.post('/api/CustManage/SetCustStatusSwitch', { CustId: data.custId, OP: 2 })
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

        onPhoneVerifiedChange(e) {
      if (confirm("Are you sure change PhoneVerified ?")) {
        let data = this.props.CustInfoData;
        axios
          .post("/api/CustManage/SetCustStatusSwitch", {
            CustId: data.custId,
            OP: 3,
          })
          .then(
            function (response) {
              //Loading.UnLoading();
              let result = response.data;
              if (result.errorCode == 0) {
                this.props.onGetCustInfoDetail();
              } else {
                alert(result.message);
              }
            }.bind(this)
          )
          .catch(
            function (error) {
              //Loading.UnLoading();
              alert(error);
            }.bind(this)
          );
      }
    }

    onEmailVerifiedChange(e) {
      if (confirm("Are you sure change EmailVerified ?")) {
        let data = this.props.CustInfoData;
        axios
          .post("/api/CustManage/SetCustStatusSwitch", {
            CustId: data.custId,
            OP: 4,
          })
          .then(
            function (response) {
              //Loading.UnLoading();
              let result = response.data;
              if (result.errorCode == 0) {
                this.props.onGetCustInfoDetail();
              } else {
                alert(result.message);
              }
            }.bind(this)
          )
          .catch(
            function (error) {
              //Loading.UnLoading();
              alert(error);
            }.bind(this)
          );
      }
    }
    onPayoutVerifitionReward(_OTPType) {
      let data = this.props.CustInfoData;
      let text = "";
      axios
        .post("/api/CustManage/CheckRewardCondination", {
          CustId: data.custId,
          OTPType: _OTPType,
        })
        .then(
          function (response) {
            let result = response.data;
            if (result.errorCode == 0) {
              text =
                result.message +
                "\r\nVerification Reward will payout immediately. \r\n";
              if (confirm(text)) {
                axios
                  .post("/api/CustManage/PayoutVerificationReward", {
                    CustId: data.custId,
                    OTPType: _OTPType,
                  })
                  .then(
                    function (response) {
                      let result = response.data;
                      if (result.errorCode == 0) {
                        this.props.onGetCustInfoDetail();
                      } else {
                        alert(result.message);
                      }
                    }.bind(this)
                  )
                  .catch(
                    function (error) {
                      alert(error);
                    }.bind(this)
                  );
              }
            } else {
            }
          }.bind(this)
        )
        .catch(function (error) {}.bind(this));
    }

        onAskReupload() {
            if (confirm('Are you sure delete Photo ?')) {
                let data = this.props.CustInfoData;
                axios.post('/api/CustManage/DelCustPhotoId', { CustId: data.custId })
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

        onNewUpload() {
            if (confirm('Are you sure upload New Photo ?')) {
                if (!this.state.file) { return alert("Please choose one Photo！"); }
                Loading.OnLoading();
                let data = this.props.CustInfoData;
                let settings = { headers: { 'content-type': 'multipart/form-data' } }
                let file = document.getElementById("uploadfile").files[0];
                let formData = new FormData();
                formData.append("File", file, file.name);
                formData.append('CustId', parseInt(data.custId));


                axios.post('/api/CustManage/UploadCustPhotoId', formData, settings)
                    .then(function (response) {
                        Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.props.onGetCustInfoDetail();
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        onResetProfile() {
            if (confirm('Are you sure reset Profile ?')) {
                let data = this.props.CustInfoData;
                axios.post('/api/CustManage/ResetProfile', { CustId: data.custId })
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

        onSetEnableDisablePromotion(e){
            if (confirm('Are you sure ' + (e==0 ?'Disable':'Enable') + ' Promotion')) {
                let data = this.props.CustInfoData;
                axios.post('/api/CustManage/SetEnableDisablePromotion', { CustId: data.custId ,EnablePromotion: e})
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

        onResetCustBankInfo() {
            if (confirm('Are you sure reset Bank Info ?')) {
                let data = this.props.CustInfoData;
                axios.post('/api/CustManage/ResetCustBankInfo', { CustId: data.custId })
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
        onResetCustUSDTnfo() {
            if (confirm('Are you sure reset USDT Info ?')) {
                let data = this.props.CustInfoData;
                axios.post('/api/CustManage/ResetCustUSDTInfo', { CustId: data.custId })
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

        onUnlock() {
            if (!this.state.reason) { return alert("Please enter Reason For Unlock！"); }
            if (confirm('Are you sure set Unlock ?')) {
                let data = this.props.CustInfoData;
                axios.post('/api/CustManage/SetUnlock', { CustId: data.custId,Reason:this.state.reason })
                    .then(function (response) {
                        //Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            alert(result.message);
                            this.cancelSetReason();
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

        onLock() {
            if (!this.state.reason) { return alert("Please enter Reason For Lock！"); }
            if (confirm('Are you sure set Lock ?')) {
                let data = this.props.CustInfoData;
                axios.post('/api/CustManage/SetLock', { CustId: data.custId,Reason:this.state.reason })
                    .then(function (response) {
                        //Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            alert(result.message);
                            this.cancelSetReason();
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

        setReason(action){
            if(!this.state.isOpen)
            {
                switch(action)
                {
                    case "lock":
                        this.setState({isOpen:true,
                            reason:"",
                            action:{Cancel:this.cancelSetReason.bind(this),
                                    EditReason:this.editReason.bind(this),
                                    Submit:this.onLock.bind(this)}});
                    break;
                    case "unlock":
                        this.setState({isOpen:true,
                            reason:"",
                            action:{Cancel:this.cancelSetReason.bind(this),
                                    EditReason:this.editReason.bind(this),
                                    Submit:this.onUnlock.bind(this)}});
                    break;
                    case "closed":
                        this.setState({isOpen:true,
                            reason:"",
                            action:{Cancel:this.cancelSetReason.bind(this),
                                    EditReason:this.editReason.bind(this),
                                    Submit:this.onClose.bind(this)}});
                    break;
                    case "kickout":
                        this.setState({isOpen:true,
                            reason:"",
                            action:{Cancel:this.cancelSetReason.bind(this),
                                    EditReason:this.editReason.bind(this),
                                    Submit:this.kickOut.bind(this)}});
                    break;
                }
            }
        }

        editReason(value){
            this.setState({reason:value});
        }

        cancelSetReason(){
            if(this.state.isOpen)
            {
                this.setState({isOpen:false,reason:"",action:null});
            }
        }

        kickOut() {
            if (!this.state.reason) { return alert("Please enter Reason For KickOut！"); }
            if (confirm('Are you sure kick Out this Customer?')) {
                let data = this.props.CustInfoData;
                axios.post('/api/CustManage/KickOut', { CustId: data.custId,Reason:this.state.reason })
                    .then(function (response) {
                        let result = response.data;
                        if (result.errorCode == 0) {
                            alert(result.message);
                            this.cancelSetReason();
                        } else {
                            alert(result.message);
                            this.cancelSetReason();
                        }
                    }.bind(this)).catch(function (error) {
                        alert(error);
                    }.bind(this));
            }
        }

        onClose() {
            if (!this.state.reason) { return alert("Please enter Reason For Closed！"); }
            if (confirm('Are you sure set Close ?')) {
                let data = this.props.CustInfoData;
                axios.post('/api/CustManage/SetClose', { CustId: data.custId,Reason:this.state.reason })
                    .then(function (response) {
                        //Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            alert(result.message);
                            this.cancelSetReason();
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

        cancelBonus(bonusId) {
            if (confirm('Are you sure Cancel Bonus ?')) {
                let data = this.props.CustInfoData;
                axios.post('/api/CustManage/CancelBonusByBonusId', { CustId: data.custId, BonusId: bonusId })
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

        onFixNegative() {
            if (confirm('Are you sure?')) {
                Loading.OnLoading();
                let data = this.props.CustInfoData;
                axios.post('/api/Tp/FixNegative', { CustId: data.custId })
                    .then(function (response) {
                        Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            alert("Fix Success")
                            this.props.onGetCustInfoDetail();
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        // setTag(){

        // }
        render() {

            let data = this.props.CustInfoData;
            let tagData = this.props.TagList;
            let tpWallet = null;
            let elgibleWithdrawal = 0;
            let promotions = null;
            let tagList = [];
            let selectTagList =[];
            let selectTagValue = [];
            let tag = "";
            let tagSelectList = null;

            if (data != null) {
                elgibleWithdrawal = data.withdrawalLimit - data.turnOver;
                if (elgibleWithdrawal < 0) {
                    elgibleWithdrawal = 0;
                }
                elgibleWithdrawal = data.balance - elgibleWithdrawal;

                if (data.tpWallet != null) {
                    tpWallet = <ul className="wallet-wrap">{data.tpWallet.map(function (wallet) {
                        return <li key={wallet.tpId} className="wallet-item table-detail">
                            <span className="wallet-name">{wallet.tpName}</span>
                            <div className="name"><span>{wallet.tpUserName}</span><button type="button" className="btn btn-copy js-copy" data-clipboard-text={wallet.tpUserName}>Copy</button></div>
                            <span className={wallet.balance < 0 ? "negative-num" : "positive-num"}>{number_format(wallet.balance)}</span>
                            {wallet.tpId == 1 && wallet.balance < 0 ?
                                <div className="button-block">
                                    <button className="btn btn-primary" onClick={this.onFixNegative.bind(this)}>Fix Negative</button>
                                </div>
                                : null}
                        </li>;
                    }.bind(this))}</ul>;

                    for (let i = 0; i < data.tpWallet.length; i++) {
                        elgibleWithdrawal += data.tpWallet[i].balance;
                    }
                }

                if (data.promotions != null) {
                    promotions = <Promotion Promotions={data.promotions} CancelBonus={this.cancelBonus.bind(this)} />;
                }

                if(data.iTagList.length > 0){
                    for(let i=0; i < data.iTagList.length;i++){
                        tagData.forEach(element=>{
                            if(element.id === data.iTagList[i]){
                                tagList.push(element.tagName);
                            }
                        })
                    }
                }
                

                tag = tagList.toString();
                tagSelectList = tagData.map(obj=>{
                    return <option key={`tag-list-${obj.id}`} value={obj.id}>{obj.tagName}</option>;
                })
                tagSelectList = <select id="selectTag" name="selectTag[]" multiple="multiple">{tagSelectList}</select>;

            }

            return (
                data == null ? <div className="text-center">
                    <div className="spinner-border" role="status">
                        <span className="sr-only">Loading...</span>
                    </div>
                </div> :
                    <div>
                        <div className="button-block">
                            {data.phoneVerified || data.emailVerified ? null : <button className="btn btn-primary" onClick={this.onResetProfile.bind(this)}>Reset Profile</button>}
                            <button className="btn btn-primary" onClick={this.onResetCustBankInfo.bind(this)}>Reset Customer Bank Info.</button>
                            <button className="btn btn-primary" onClick={this.onResetCustUSDTnfo.bind(this)}>Reset Customer USDT Info.</button>
                            {data.status == 1 && data.agent != null && subFuncKeys.indexOf("ca_acc_lock") > -1 ? <button className="btn btn-primary" onClick={this.setReason.bind(this,"lock")}>Lock</button> : null}
                            {(data.status == 1 || data.status == 2 || data.status == 4) && data.agent != null && subFuncKeys.indexOf("ca_acc_close") > -1 ? <button className="btn btn-primary" onClick={this.setReason.bind(this,"closed")}>Closed</button> : null}
                            {(data.status == 2 || data.status == 4) && data.agent != null && subFuncKeys.indexOf("ca_unlock") > -1 ? <button className="btn btn-primary" onClick={this.setReason.bind(this,"unlock")}>Unlock</button> : null}
                            {subFuncKeys.indexOf("ca_kickout") > -1 ? <button className="btn btn-primary" onClick={this.setReason.bind(this,"kickout")}>KickOut</button> : null}
                            {(data.enablePromotion == 0) && subFuncKeys.indexOf("ca_promotion") > -1 ? <button className="btn btn-primary" onClick={this.onSetEnableDisablePromotion.bind(this,1)}>Enable Promotion</button> : null}
                            {(data.enablePromotion == 1) && subFuncKeys.indexOf("ca_promotion") > -1 ? <button className="btn btn-primary" onClick={this.onSetEnableDisablePromotion.bind(this,0)}>Disable Promotion</button> : null}
                        </div>
                        <div className="flex-wrap">
                            <table className="table table-detail ">
                                <tbody>
                                    <tr>
                                        <th scope="row">Site Name</th>
                                        <td>{data.siteName}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">User Name</th>
                                        <td>{data.userName}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">User Level</th>
                                        {
                                            this.props.isEdit.Level ? (
                                                <td>
                                                    <input type="number" min="1" defaultValue={data.userLevel} onChange={(e) => this.props.editOnChange(e.target.value, "Level")} />
                                                    <div className="button-action float-right">
                                                        <button className="btn btn-save" onClick={this.props.editUpdate}>
                                                            <i className="icon-save"></i>
                                                        </button>
                                                        <button className="btn btn-icon-cancel" onClick={this.props.onClickEdit.bind(null, this.props.isEdit.Level, "Level")}>
                                                            <i className="icon-cancel"></i>
                                                        </button>
                                                    </div>
                                                </td>
                                            ) : (
                                                    <td>
                                                        {data.userLevel}
                                                        {
                                                            subFuncKeys.includes('ca_userEdit') ? (
                                                                <div className="button-action float-right">
                                                                    <button className="btn btn-edit" onClick={this.props.onClickEdit.bind(null, this.props.isEdit.Level, "Level")}>
                                                                        <i className="icon-edit"></i>
                                                                    </button>
                                                                </div >
                                                            ) : null
                                                        }
                                                    </td>
                                                )
                                        }
                                    </tr>
                                    <tr>
                                        <th scope="row">Point Level</th>
                                        <td>{data.vipLevel}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">
                                            Vip Level&nbsp;<span class="status status-pending">New</span>
                                        </th>
                                        <td>{data.levelName}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Currency</th>
                                        <td>{i18n["lbl_Currency" + data.currencyId]}</td></tr>
                                    <tr>
                                        <th scope="row">Agent Name</th>
                                        <td>{data.agent}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Affiliates ID</th>
                                        <td>{(typeof data.affiliateId === 'undefined' || data.affiliateId === 0) ? "N/A" : data.affiliateId}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Affiliates Code</th>
                                        <td>{(typeof data.affiliateCode === null) ? "" : data.affiliateCode}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Prefer Languages</th>
                                        <td>{data.userLang}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Full Name</th>
                                        {
                                            this.props.isEdit.FullName ? (
                                                <td>
                                                    <input type="text" defaultValue={data.accountName} onChange={(e) => this.props.editOnChange(e.target.value, "FullName")} />
                                                    <div className="button-action float-right">
                                                        <button className="btn btn-save" onClick={this.props.editUpdate}>
                                                            <i className="icon-save"></i>
                                                        </button>
                                                        <button className="btn btn-icon-cancel" onClick={this.props.onClickEdit.bind(null, this.props.isEdit.FullName, "FullName")}>
                                                            <i className="icon-cancel"></i>
                                                        </button>
                                                    </div>
                                                </td>
                                            ) : (
                                                    <td>
                                                        {data.accountName}
                                                        {
                                                            subFuncKeys.includes('ca_userEdit') ? (
                                                                <div className="button-action float-right">
                                                                    <button className="btn btn-edit" onClick={this.props.onClickEdit.bind(null, this.props.isEdit.FullName, "FullName")}>
                                                                        <i className="icon-edit"></i>
                                                                    </button>
                                                                </div >
                                                            ) : null
                                                        }
                                                    </td>
                                                )
                                        }
                                    </tr>
                                    <tr>
                                        <th scope="row">Birthday (M/D/Y)</th>

                                        {
                                            this.props.isEdit.Birthday ? (
                                                <td>
                                                    <input type="text" style={{ width: "50px", marginRight: "5px" }} maxLength="2" placeholder="M" defaultValue={data.birth_m} onChange={(e) => this.props.editOnChange(e.target.value, "Birthday_M")} />
                                                    <input type="text" style={{ width: "50px", marginRight: "5px" }} maxLength="2" placeholder="D" defaultValue={data.birth_d} onChange={(e) => this.props.editOnChange(e.target.value, "Birthday_D")} />
                                                    <input type="text" style={{ width: "50px", marginRight: "5px" }} maxLength="4" placeholder="Y" defaultValue={data.birth_y} onChange={(e) => this.props.editOnChange(e.target.value, "Birthday_Y")} />
                                                    <div className="button-action float-right">
                                                        <button className="btn btn-save" onClick={this.props.editUpdate}>
                                                            <i className="icon-save"></i>
                                                        </button>
                                                        <button className="btn btn-icon-cancel" onClick={this.props.onClickEdit.bind(null, this.props.isEdit.Birthday, "Birthday")}>
                                                            <i className="icon-cancel"></i>
                                                        </button>
                                                    </div>
                                                </td>
                                            ) : (
                                                    <td>
                                                        {(!isNaN(data.birth_y) && !isNaN(data.birth_m) && !isNaN(data.birth_d)) ? data.birth_m + '/' + data.birth_d + '/' + data.birth_y : "N/A"}
                                                        {
                                                            subFuncKeys.includes('ca_userEdit') ? (
                                                                <div className="button-action float-right">
                                                                    <button className="btn btn-edit" onClick={this.props.onClickEdit.bind(null, this.props.isEdit.Birthday, "Birthday")}>
                                                                        <i className="icon-edit"></i>
                                                                    </button>
                                                                </div >
                                                            ) : null
                                                        }
                                                    </td>
                                                )
                                        }
                                    </tr>
                                    <tr>
                                        <th scope="row">Country Code</th>
                                        <td>{data.countryCode}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Phone Number</th>
                                        {
                                            !data.phoneVerified ?
                                                this.props.isEdit.PhoneNumber ? (
                                                    <td>
                                                        {"(" + ((typeof data.areaCode === 'undefined') ? "N/A" : data.areaCode) + ") "}<input type="text" defaultValue={data.phoneNo} onChange={(e) => this.props.editOnChange(e.target.value, "PhoneNumber")} />
                                                        <div className="button-action float-right">
                                                            <button className="btn btn-save" onClick={this.props.editUpdate}>
                                                                <i className="icon-save"></i>
                                                            </button>
                                                            <button className="btn btn-icon-cancel" onClick={this.props.onClickEdit.bind(null, this.props.isEdit.PhoneNumber, "PhoneNumber")}>
                                                                <i className="icon-cancel"></i>
                                                            </button>
                                                        </div>
                                                    </td>
                                                ) : (
                                                        <td>
                                                            {"(" + ((typeof data.areaCode === 'undefined') ? "N/A" : data.areaCode) + ") " + ((typeof data.phoneNo === 'undefined') ? "N/A" : data.phoneNo)}
                                                            {
                                                                subFuncKeys.includes('ca_userEdit') ? (
                                                                    <div className="button-action float-right">
                                                                        <button className="btn btn-edit" onClick={this.props.onClickEdit.bind(null, this.props.isEdit.PhoneNumber, "PhoneNumber")}>
                                                                            <i className="icon-edit"></i>
                                                                        </button>
                                                                    </div >
                                                                ) : null
                                                            }
                                                        </td>
                                                    )
                                                : <td>
                                                    {"(" + ((typeof data.areaCode === 'undefined') ? "N/A" : data.areaCode) + ") " + ((typeof data.phoneNo === 'undefined') ? "N/A" : data.phoneNo)}
                                                </td>
                                        }
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
                                            {
                                                subFuncKeys.includes('ca_isKyc') ? (
                                                    <div className="switch__container">
                                                        <input id="switch-isKyc" className="switch switch--flat" checked={data.isKyc} type="checkbox" onChange={this.onKycChange.bind(this)} />
                                                        <label htmlFor="switch-isKyc"></label>
                                                    </div>
                                                ) : null
                                            }
                                        </td>
                                    </tr>
                                    <tr>
                                        <th scope="row">IsRisk</th>
                                        <td>
                                            {data.isRisk ? "Yes" : "No"}
                                            {
                                                subFuncKeys.includes('ca_isRisk') ? (
                                                    <div className="switch__container">
                                                        <input id="switch-isRisk" className="switch switch--flat" checked={data.isRisk} type="checkbox" onChange={this.onRiskChange.bind(this)} />
                                                        <label htmlFor="switch-isRisk"></label>
                                                    </div>
                                                ) : null
                                            }
                                        </td>
                                    </tr>
                                    <tr>
                                        <th scope="row">
                    PhoneVerified
                    {this.props.isShowRewardBotton.SMS ? (
                      <button
                        className="btn btn-copy float-right"
                        onClick={this.onPayoutVerifitionReward.bind(this, 0)}
                      >
                        Reward
                      </button>
                    ) : null}
                  </th>
                                        <td>
                                            {data.phoneVerified ? "Yes" : "No"}
                                            {
                                                subFuncKeys.includes('ca_phoneVerified') ? (
                                                    <div className="switch__container">
                                                        <input id="switch-phoneVerified" className="switch switch--flat" checked={data.phoneVerified} type="checkbox" onChange={this.onPhoneVerifiedChange.bind(this)} />
                                                        <label htmlFor="switch-phoneVerified"></label>
                                                    </div>
                                                ) : null
                                            }
                                        </td>
                                    </tr>
                                    <tr>
                                        <th scope="row">
                    EmailVerified
                    {this.props.isShowRewardBotton.EMAIL ? (
                      <button 
                      className="btn btn-copy float-right"
                      onClick={this.onPayoutVerifitionReward.bind(this, 1)}
                      >
                        Reward
                      </button>
                    ) : null}
                  </th>
                                        <td>
                                            {data.emailVerified ? "Yes" : "No"}
                                            {
                                                subFuncKeys.includes('ca_emailVerified') ? (
                                                    <div className="switch__container">
                                                        <input id="switch-emailVerified" className="switch switch--flat" checked={data.emailVerified} type="checkbox" onChange={this.onEmailVerifiedChange.bind(this)} />
                                                        <label htmlFor="switch-emailVerified"></label>
                                                    </div>
                                                ) : null
                                            }
                                        </td>
                                    </tr>
                                    <tr>
                                        <th scope="row">VerifiedReward</th>
                                        <td>{data.verificationReward}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Iovation</th>
                                        <td>{data.iovation ? CustIovationOp[data.iovation] : null}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Tag</th>
                                        <td>{tag}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Status</th>
                                        <td>{CustStatusOp[data.status]}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Registration Time (Adjusted to GMT+8)</th>
                                        <td>{moment.utc(data.regTime).add(8,'hour').format('l LTS')}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Last Login Time (Adjusted to GMT+8)</th>
                                        <td>{moment.utc(data.lastLoginTime).add(8,'hour').format('l LTS')}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">First Deposit Time (Adjusted to GMT+8)</th>
                                        <td>{data.firstDepositTime}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Last Activity Time (Adjusted to GMT+8)</th>
                                        <td>{(typeof data.reloadTime === 'undefined') ? "N/A" : moment.utc(data.reloadTime).add(8,'hour').format('l LTS')}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Last Activity IP</th>
                                        <td>{(typeof data.ip === 'undefined') ? "N/A" : data.ip}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">E-Mail</th>
                                        {
                                            !data.emailVerified ?
                                                this.props.isEdit.EMail ? (
                                                    <td>
                                                        <input type="text" defaultValue={data.email} onChange={(e) => this.props.editOnChange(e.target.value, "EMail")} />
                                                        <div className="button-action float-right">
                                                            <button className="btn btn-save" onClick={this.props.editUpdate}>
                                                                <i className="icon-save"></i>
                                                            </button>
                                                            <button className="btn btn-icon-cancel" onClick={this.props.onClickEdit.bind(null, this.props.isEdit.EMail, "EMail")}>
                                                                <i className="icon-cancel"></i>
                                                            </button>
                                                        </div>
                                                    </td>
                                                ) : (
                                                        <td>
                                                            {data.email ? data.email : "N/A"}
                                                            {
                                                                subFuncKeys.includes('ca_userEdit') ? (
                                                                    <div className="button-action float-right">
                                                                        <button className="btn btn-edit" onClick={this.props.onClickEdit.bind(null, this.props.isEdit.EMail, "EMail")}>
                                                                            <i className="icon-edit"></i>
                                                                        </button>
                                                                    </div >
                                                                ) : null
                                                            }
                                                        </td>
                                                    )
                                                : <td>
                                                    {data.email ? data.email : "N/A"}
                                                </td>
                                        }
                                    </tr>
                                    <tr>
                                        <th scope="row">Mail Wallet Amount</th>
                                        <td>{number_format(data.balance)}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Last Deposit Time (Adjusted to GMT+8)</th>
                                        <td>{data.lastDepositTime}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Deposit Summary</th>
                                        <td>{number_format(data.depositSum)}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Last Withdrawal Time (Adjusted to GMT+8)</th>
                                        <td>{data.lastWithdrawlTime}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Withdrawal Summary</th>
                                        <td>{number_format(data.withdrawlSum)}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Effective Betting Summary</th>
                                        <td>{number_format(data.effectiveBetSum)}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Winnings</th>
                                        <td>{number_format(data.winnings)}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Losses</th>
                                        <td>{number_format(data.losses)}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">WithdrawalLimit</th>
                                        <td>{number_format(data.withdrawalLimit)}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">TurnOver</th>
                                        <td>{number_format(data.turnOver)}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Eligible Withdrawal</th>
                                        <td>{number_format(elgibleWithdrawal)}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Last Betting Time (Adjusted to GMT+8)</th>
                                        <td>{data.lastBettingTime}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Total Adjustment</th>
                                        <td>{number_format(data.totalAdjustment)}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Total Bonus Claimed</th>
                                        <td>{number_format(data.totalBonusClaimed)}</td>
                                    </tr>
                                </tbody>
                            </table>
                            {data.RebateInfo != null ? <RebateInfo RebateInfo={data.RebateInfo} /> : null}
                            {data.BonusWallet != null ? <BonusWallet BonusWallet={data.BonusWallet} /> : null}
                            {promotions}
                            <div className="table-responsive-md table">
                                <span class="label">Photo</span>
                                <table className="table table-striped">
                                    <thead>
                                        <tr>
                                            <th scope="col">Cust Photo</th>
                                            <th scope="col">Action</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {data.hasPhotoId ?
                                            <tr>
                                                <td>
                                                    <div className="id-image">
                                                        <img src={"/api/CustManage/GetCustPhotoId?CustId=" + data.custId} alt="Identity Card" />
                                                    </div>
                                                </td>
                                                <td>
                                                    <button className="btn btn-delete" disabled={data.isKyc} onClick={!data.isKyc ? this.onAskReupload.bind(this) : null}>Ask Re-Upload</button>
                                                </td>
                                            </tr>
                                            :
                                            <tr>
                                                <td>
                                                    <input id="uploadfile" className="form-control" type="file" accept=".png, .jpg, .jpeg" onChange={(e) => { this.EditFiled(e.target.value, 'file') }} disabled={data.isKyc} />
                                                </td>
                                                <td>
                                                    <button className="btn btn-add" disabled={data.isKyc} onClick={!data.isKyc ? this.onNewUpload.bind(this) : null}>Upload</button>
                                                </td>
                                            </tr>
                                        }
                                    </tbody>
                                </table>
                            </div>
                            {
				this.state.isOpen ?
					<SetCustStatusNote
						NewDatas={{reason:this.state.reason}}
						Action={this.state.action}
					/>
					: null
			}


                        </div>
                        {tpWallet}
                    </div>
                    
            )
        }
    }

    class CustInfoPanel extends React.Component {
        _Tabs = ["Customer Info", "Bonus Log", "Financial Log", "Vip Rebate Log", "Statement", "Deposit Info", "Transfer Info", "Withdrawal Info", "Adjustment Info", "Login Log", "Reset Password", "Notes", "Personal Message", "Turnover", "Admin User Log", "Bank Info Log", "Crypto Wallet Log", "Referral History", "VIP Level", "Promotion Report","Tag","Withdrawal Limit Log"];
        _isEdit = { Level: false, FullName: false, Birthday: false, PhoneNumber: false, EMail: false, Tag: false };
        _newData = { Level: null, FullName: null, Birthday_Y: null, Birthday_M: null, Birthday_D: null, PhoneNumber: null, EMail: null ,Tag:null};
        _isShowRewardBotton = { SMS: false, EMAIL: false };
        constructor(props) {
            super(props);
            this.state = { CustInfoData: null, TabIdx: 0, BonusLog: {}, FinancialLog: {}, RebateLog: {}, BankLogData: [], CryptoWalletLogData :[], Notes: null, TabName: this._Tabs[0], isEdit: Object.assign({}, this._isEdit), newData: Object.assign({}, this._newData), ReferralHistory: {},TagList:{},isShowRewardBotton: Object.assign({}, this._isShowRewardBotton), };
        }

        onTab(idx, tabName) {
            this.setState({ TabIdx: idx, TabName: tabName });
        }

        componentDidMount() {
            this.GetAllTagData();
            this.onGetCustInfoDetail();
        }

        GetPromotionCategory() {
            Loading.OnLoading();
            axios.post('/api/Payment/GetPromotionCategory')
                .then(function (response) {
                    Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({ PromotionCategoryData: result.data });
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    Loading.UnLoading();
                    alert(error);
                }.bind(this));
        }

        GetBonusLog(PageNumber) {
            Loading.OnLoading();
            let pageNumber = PageNumber || this.state.BonusLogData.pageNumber;
            let CustId = getUrlParameter("custId");
            const {UTCStartDate,UTCEndDate,PromoCategoryId,RefTransId} = this.state.BonusLog;
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                axios.post('/api/Payment/GetBonusLog', { StartDate: UTCStartDate,
                                                        EndDate: UTCEndDate, 
                                                        CustId: iCustId, 
                                                        PageNumber: pageNumber,
                                                        PromoCategoryId: PromoCategoryId,
                                                        RefTransId: RefTransId})
                    .then(function (response) {
                        Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ BonusLogData: result });
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        // GetRebateLog(PageNumber) {
        //     Loading.OnLoading();
        //     let pageNumber = PageNumber || this.state.RebateLogData.pageNumber;
        //     let CustId = getUrlParameter("custId");
        //     if (!isNaN(CustId)) {
        //         let iCustId = parseInt(CustId);
        //         axios.post('/api/Payment/GetRebateLogByCust', { ...this.state.RebateLog, CustId: iCustId, PageNumber: pageNumber })
        //             .then(function (response) {
        //                 Loading.UnLoading();
        //                 let result = response.data;
        //                 if (result.errorCode == 0) {
        //                     this.setState({ RebateLogData: result });
        //                 } else {
        //                     alert(result.message);
        //                 }
        //             }.bind(this)).catch(function (error) {
        //                 Loading.UnLoading();
        //                 alert(error);
        //             }.bind(this));
        //     }
        // }
        GetRebateLog(PageNumber) {
            Loading.OnLoading();
            let pageNumber = PageNumber || this.state.RebateLogData.pageNumber;
            let CustId = getUrlParameter("custId");
            const {UTCStartDate,UTCEndDate} = this.state.RebateLog;
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                axios.post('/api/Payment/GetVipRebateInfoListByCustId', { StartDate: UTCStartDate,
                                                                        EndDate: UTCEndDate, 
                                                                        CustId: iCustId, 
                                                                        PageNumber: pageNumber })
                    .then(function (response) {
                        Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ RebateLogData: result });
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        GetReferralHistory(PageNumber) {
            Loading.OnLoading();
            let pageNumber = PageNumber || this.state.ReferralHistoryData.pageNumber;
            let CustId = getUrlParameter("custId");
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                axios.post('/api/ReferralManagement/GetRefFriendList', { CustId: iCustId, PageNumber: pageNumber })
                    .then(function (response) {
                        Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ ReferralHistoryData: result });
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        GetFinancialLog(PageNumber) {
            Loading.OnLoading();
            let pageNumber = PageNumber || this.state.FinancialLogData.pageNumber;
            let CustId = getUrlParameter("custId");
            const {UTCStartDate,UTCEndDate,RefTransId} = this.state.FinancialLog;
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                axios.post('/api/Payment/GetFinancialLog', { StartDate: UTCStartDate,
                                                            EndDate: UTCEndDate, 
                                                            CustId: iCustId, 
                                                            PageNumber: pageNumber,
                                                            RefTransId: RefTransId })
                    .then(function (response) {
                        Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ FinancialLogData: result });
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        GetDepositListByCust(PageNumber) {
            let pageNumber = PageNumber || this.state.DepositListData.pageNumber;
            let CustId = getUrlParameter("custId");
            const {UTCStartDate,UTCEndDate,Status} = this.state.DepositList;
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                axios.post('/api/Deposit/GetDepositListByCust', { StartDate: UTCStartDate,
                                                                EndDate: UTCEndDate,
                                                                Status: Status,
                                                                CustId: iCustId, 
                                                                PageNumber: pageNumber,
                                                                PageSize: 10 })
                    .then(function (response) {
                        //Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ DepositListData: result });
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
            let pageNumber = PageNumber || this.state.WithdrawalListData.pageNumber;
            let CustId = getUrlParameter("custId");

            const {UTCStartDate,UTCEndDate,Status} = this.state.WithdrawalInfo;
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                
                axios.post('/api/Withdrawal/GetWithdrawalListByCust', { StartDate: UTCStartDate,
                                                                    EndDate: UTCEndDate,
                                                                    Status: Status,
                                                                    CustId: iCustId, 
                                                                    PageNumber: pageNumber,
                                                                    PageSize:10 })
                    .then(function (response) {
                        //Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ WithdrawalListData: result });
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
            let pageNumber = PageNumber || this.state.TpTransferInfoData.pageNumber;
            let CustId = getUrlParameter("custId");
            const {UTCStartDate,UTCEndDate} = this.state.TpTransferInfo;
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                axios.post('/api/Tp/GetTpTransferInfoByCust', { StartDate: UTCStartDate,
                                                            EndDate: UTCEndDate, 
                                                            CustId: iCustId, 
                                                            PageNumber: pageNumber })
                    .then(function (response) {
                        //Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ TpTransferInfoData: result });
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        //Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        GetAdjustmentInfoByCust(PageNumber) {
            Loading.OnLoading();
            let pageNumber = PageNumber || this.state.AdjustmentInfoData.pageNumber;
            let CustId = getUrlParameter("custId");
            const {UTCStartDate,UTCEndDate} = this.state.AdjustmentInfo;
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                axios.post('/api/PaymentAdjustmentInfo/GetAdjustmentInfoByCust', { StartDate: UTCStartDate,
                                                                                EndDate: UTCEndDate, 
                                                                                CustId: iCustId, 
                                                                                PageNumber: pageNumber })
                    .then(function (response) {
                        Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ AdjustmentInfoData: result });
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        GetLoginLogByCust(PageNumber) {
            let pageNumber = PageNumber || this.state.LoginLogData.pageNumber;
            let CustId = getUrlParameter("custId");
            const {UTCStartDate,UTCEndDate} = this.state.LoginLog;
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                axios.post('/api/CustManage/GetLoginLogByCust', {StartDate: UTCStartDate,
                                                                EndDate: UTCEndDate,
                                                                CustId: iCustId, 
                                                                PageNumber: pageNumber })
                    .then(function (response) {
                        //Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ LoginLogData: result });
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        //Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        GetAdminUserLogByCust(PageNumber) {
            let pageNumber = PageNumber || this.state.AdminUserLogData.pageNumber;
            let CustId = getUrlParameter("custId");
            const {UTCStartDate,UTCEndDate} = this.state.AdminUserLog;
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                axios.post('/api/Common/GetAdminOperateLog', { StartDate: UTCStartDate,
                                                            EndDate: UTCEndDate, 
                                                            CustId: iCustId, 
                                                            PageNumber: pageNumber })
                    .then(function (response) {
                        //Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ AdminUserLogData: result });
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
            if (!isNaN(CustId) && note.trim()) {
                let iCustId = parseInt(CustId);
                axios.post('/api/CustManage/AddCustNote', { CustId: iCustId, Note: note })
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
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                axios.post('/api/CustManage/GetCustNotes', { CustId: iCustId })
                    .then(function (response) {
                        //Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ Notes: result.data });
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        //Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        SetCustNote(id, note, type) {
            let CustId = getUrlParameter("custId"), confirminfo = (type === 1) ? 'Are you sure edit?' : 'Are you sure delete?';
            if (!isNaN(CustId) && confirm(confirminfo)) {
                let iCustId = parseInt(CustId), noteId = parseInt(id), action = parseInt(type);
                axios.post('/api/CustManage/SetCustNotes', { CustId: iCustId, Note: note, NoteId: noteId, Type: type })
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

        onGetCustInfoDetail() {
            let CustId = getUrlParameter("custId");
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                axios.post('/api/CustManage/GetCustInfoDetail', { CustId: iCustId })
                    .then(function (response) {
                        //Loading.UnLoading();
                        let result = response.data;
                        let tagList=[];
                        if (result.errorCode == 0) {
                            this.setState({ CustInfoData: result.data }, function () {
                                let _clipboard = new ClipboardJS(".js-copy");
                            });
                            this.GetBonusWalletLog();
                            this.GetRebateInfoLog();
                            this.onGetRewardBottonStatus();
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        //Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }
    onGetRewardBottonStatus() {
      let CustId = getUrlParameter("custId");
      if (!isNaN(CustId)) {
        let iCustId = parseInt(CustId);
        let obj = this.state.isShowRewardBotton;
        obj["SMS"] = false;
        obj["EMAIL"] = false;
        axios
          .post("/api/CustManage/CheckRewardCondination", {
            CustId: iCustId,
            OTPType: 0,
          }) //SMS
          .then(
            function (response) {
              //Loading.UnLoading();
              let result = response.data;
              if (result.errorCode == 0) {
                obj["SMS"] = true;
              } else {
                //alert(result.message);
              }
              this.setState(obj);
            }.bind(this)
          )
          .catch(
            function (error) {
              //Loading.UnLoading();
              alert(error);
            }.bind(this)
          );
        axios
          .post("/api/CustManage/CheckRewardCondination", {
            CustId: iCustId,
            OTPType: 1,
          }) //EMAIL
          .then(
            function (response) {
              //Loading.UnLoading();
              let result = response.data;
              if (result.errorCode == 0) {
                obj["EMAIL"] = true;
              } else {
                //alert(result.message);
              }
              this.setState(obj);
            }.bind(this)
          )
          .catch(
            function (error) {
              //Loading.UnLoading();
              alert(error);
            }.bind(this)
          );
      }
    }
        onChange(e, TabName) {
            let Obj = this.state[TabName];
            Obj[event.target.name] = event.target.value;

            let obj = this.state;
            obj[TabName] = Obj;
            this.setState(obj);
        }

        onChangeValue(TabName, Obj) {
            let obj = this.state;
            obj[TabName] = Obj;
            this.setState(obj);
        }

        onClickEdit = (isEdit, Col) => {
            let obj = this.state.isEdit;
            obj[Col] = !isEdit;
            this.setState(obj);
            if (isEdit) {
                let obj1 = this.state.newData;
                if (Col === "Birthday") {
                    obj1["Birthday_D"] = null;
                    obj1["Birthday_M"] = null;
                    obj1["Birthday_Y"] = null;
                } else {
                    obj1[Col] = null;
                }
                this.setState(obj1)
            }
        }

        editOnChange = (value, Col) => {
            let obj = this.state.newData;
            obj[Col] = value.trim() ? value.trim() : null;
            this.setState(obj);
        }
        editUpdate = () => {
            let CustId = getUrlParameter("custId"),
                data = this.state.CustInfoData;
            if (!isNaN(CustId)) {

                if (this.state.newData.Level && this.state.newData.Level <= 0) return alert("Minimum is 1");

                if (confirm('Are you sure?')) {
                    let Level = (this.state.newData.Level) ? parseInt(this.state.newData.Level, 10) : null,
                        FullName = this.state.newData.FullName,
                        Birthday_D = (this.state.newData.Birthday_D) ? parseInt(this.state.newData.Birthday_D, 10) : null,
                        Birthday_M = (this.state.newData.Birthday_M) ? parseInt(this.state.newData.Birthday_M, 10) : null,
                        Birthday_Y = (this.state.newData.Birthday_Y) ? parseInt(this.state.newData.Birthday_Y, 10) : null,
                        PhoneNumber = this.state.newData.PhoneNumber,
                        EMail = this.state.newData.EMail,
                        iCustId = parseInt(CustId, 10),
                        _isEdit = Object.assign({}, this._isEdit),
                        _newData = Object.assign({}, this._newData);

                    if (Level != data.userLevel || FullName != data.accountName || Birthday_D != data.birth_d || Birthday_M != data.birth_m || Birthday_Y != data.birth_y || PhoneNumber != data.phoneNo || EMail != data.email) {
                        axios.post('/api/CustManage/SetCustInfo', { CustId: iCustId, Level: Level, FullName: FullName, Birthday_Y: Birthday_Y, Birthday_M: Birthday_M, Birthday_D: Birthday_D, PhoneNumber: PhoneNumber, EMail: EMail })
                            .then(function (response) {
                                //Loading.UnLoading();
                                let result = response.data;
                                if (result.errorCode == 0) {
                                    let newCustInfo = { ...this.state.CustInfoData };
                                    newCustInfo['userLevel'] = (Level) ? Level : data.userLevel;
                                    newCustInfo['accountName'] = (FullName) ? FullName : data.accountName;
                                    newCustInfo['birth_d'] = (Birthday_D) ? Birthday_D : data.birth_d;
                                    newCustInfo['birth_m'] = (Birthday_M) ? Birthday_M : data.birth_m;
                                    newCustInfo['birth_y'] = (Birthday_Y) ? Birthday_Y : data.birth_y;
                                    newCustInfo['phoneNo'] = (PhoneNumber) ? PhoneNumber : data.phoneNo;
                                    newCustInfo['email'] = (EMail) ? EMail : data.email;
                                    this.setState({
                                        isEdit: _isEdit,
                                        CustInfoData: newCustInfo,
                                        newData: _newData
                                    })
                                } else {
                                    alert(result.message);
                                }
                            }.bind(this)).catch(function (error) {
                                //Loading.UnLoading();
                                alert(error);
                            }.bind(this));
                    } else {
                        this.setState({
                            isEdit: _isEdit,
                            newData: _newData
                        })
                    }
                }
            } else if (!isNaN(Level) && Level <= 0) {
                alert("Minimum is 1");
            }
        }

        GetBankLog() {
            let CustId = getUrlParameter("custId");
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                Loading.OnLoading();
                axios.post('/api/CustManage/GetBankInfoLogList', { CustId: iCustId })
                    .then(function (response) {
                        Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ BankLogData: result.data });
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        GetCryptoWalletLog() {
            let CustId = getUrlParameter("custId");
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                Loading.OnLoading();
                axios.post('/api/CustManage/GetCryptoWalletLog', { CustId: iCustId })
                    .then(function (response) {
                        Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({ CryptoWalletLogData: result.data });
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                    Loading.UnLoading();
                    alert(error);
                }.bind(this));
            }
        }

        GetBonusWalletLog() {
            let CustId = getUrlParameter("custId");
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                // Loading.OnLoading();
                axios.post('/api/CustManage/GetCustBonus', { CustId: iCustId })
                    .then(function (response) {
                        // Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({
                                ...this.state,
                                CustInfoData:
                                {
                                    ...this.state.CustInfoData,
                                    BonusWallet: result.data
                                }
                            }
                            );
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        // Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        GetRebateInfoLog() {
            let CustId = getUrlParameter("custId");
            if (!isNaN(CustId)) {
                let iCustId = parseInt(CustId);
                // Loading.OnLoading();
                axios.post('/api/CustManage/GetRebateInfoByCust', { CustId: iCustId })
                    .then(function (response) {
                        // Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({
                                ...this.state,
                                CustInfoData:
                                {
                                    ...this.state.CustInfoData,
                                    RebateInfo: result.data
                                }
                            }
                            );
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        // Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        GetAllTagData(){
            axios.post('/api/CustManage/GetAllTag')
                    .then(function (response) {
                        //Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({
                                ...this.state,
                                TagList:result.data
                            })
                            //this.buildSelectTag();
                            //console.log(result.data)
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
                        //Loading.UnLoading();
                        alert(error);
                    }.bind(this));
        }
        render() {
            let dataContainer = null;
            let TabIdx = this.state.TabIdx;
            let TabName = this.state.TabName;
            if (TabName == this._Tabs[0]) {
                dataContainer = <CustInfo CustInfoData={this.state.CustInfoData} onGetCustInfoDetail={this.onGetCustInfoDetail.bind(this)} isEdit={this.state.isEdit} onClickEdit={this.onClickEdit} editOnChange={this.editOnChange} editUpdate={this.editUpdate} TagList={this.state.TagList} isShowRewardBotton={this.state.isShowRewardBotton}/>;
            } else if (TabName == this._Tabs[1]) {
                dataContainer = <BonusLog GetPromotionCategory={this.GetPromotionCategory.bind(this)} PromotionCategoryData={this.state.PromotionCategoryData} GetBonusLog={this.GetBonusLog.bind(this)} searchData={this.state.BonusLogData} BonusLog={this.state.BonusLog} onChangeValue={this.onChangeValue.bind(this)} />;
            } else if (TabName == this._Tabs[2]) {
                dataContainer = <FinancialLog GetFinancialLog={this.GetFinancialLog.bind(this)} searchData={this.state.FinancialLogData} FinancialLog={this.state.FinancialLog} onChangeValue={this.onChangeValue.bind(this)} />;
            } else if (TabName == this._Tabs[3]) {
                dataContainer = <RebateLog GetRebateLog={this.GetRebateLog.bind(this)} searchData={this.state.RebateLogData} RebateLog={this.state.RebateLog} onChangeValue={this.onChangeValue.bind(this)} />;
            } else if (TabName == this._Tabs[4]) {
                dataContainer = <TpBetRecord />
            } else if (TabName == this._Tabs[5]) {
                dataContainer = <DepositInfo GetDepositListByCust={this.GetDepositListByCust.bind(this)} searchData={this.state.DepositListData} DepositList={this.state.DepositList} onChangeValue={this.onChangeValue.bind(this)} />;
            } else if (TabName == this._Tabs[6]) {
                dataContainer = <TpTransferInfo GetTpTransferInfoByCust={this.GetTpTransferInfoByCust.bind(this)} searchData={this.state.TpTransferInfoData} TpTransferInfo={this.state.TpTransferInfo} onChangeValue={this.onChangeValue.bind(this)} />;
            } else if (TabName == this._Tabs[7]) {
                dataContainer = <WithdrawalInfo GetWithdrawalListByCust={this.GetWithdrawalListByCust.bind(this)} searchData={this.state.WithdrawalListData} WithdrawalInfo={this.state.WithdrawalInfo} onChangeValue={this.onChangeValue.bind(this)} />;
            } else if (TabName == this._Tabs[8]) {
                dataContainer = <AdjustmentInfo GetAdjustmentInfoByCust={this.GetAdjustmentInfoByCust.bind(this)} searchData={this.state.AdjustmentInfoData} AdjustmentInfo={this.state.AdjustmentInfo} onChangeValue={this.onChangeValue.bind(this)} />;
            } else if (TabName == this._Tabs[9]) {
                dataContainer = <LoginLog GetLoginLogByCust={this.GetLoginLogByCust.bind(this)} searchData={this.state.LoginLogData} LoginLog={this.state.LoginLog} onChangeValue={this.onChangeValue.bind(this)} />;
            } else if (TabName == this._Tabs[10]) {
                dataContainer = <ResetPwd />;
            } else if (TabName == this._Tabs[11]) {
                dataContainer = <Notes AddCustNote={this.AddCustNote.bind(this)} Notes={this.state.Notes} GetCustNotes={this.GetCustNotes.bind(this)} SetCustNote={this.SetCustNote.bind(this)} />;
            } else if (TabName == this._Tabs[12]) {
                dataContainer = <PersonalMsg />
            } else if (TabName == this._Tabs[13]) {
                dataContainer = <TurnoverReport />
            } else if (TabName == this._Tabs[14]) {
                dataContainer = <AdminUserLog GetAdminUserLogByCust={this.GetAdminUserLogByCust.bind(this)} searchData={this.state.AdminUserLogData} AdminUserLog={this.state.AdminUserLog} onChangeValue={this.onChangeValue.bind(this)} />
            } else if (TabName == this._Tabs[15]) {
                dataContainer = <BankLog GetBankLog={this.GetBankLog.bind(this)} searchData={this.state.BankLogData} CurrencyId={this.state.CustInfoData.currencyId}/>
            } else if (TabName == this._Tabs[16]) {
                dataContainer = <CryptoWalletLog GetCryptoWalletLog={this.GetCryptoWalletLog.bind(this)} searchData={this.state.CryptoWalletLogData}/>
            } else if (TabName == this._Tabs[17]) {
                dataContainer = <ReferralHistory GetReferralHistory={this.GetReferralHistory.bind(this)} searchData={this.state.ReferralHistoryData} ReferralHistory={this.state.ReferralHistory} onChangeValue={this.onChangeValue.bind(this)} />
            } else if (TabName == this._Tabs[18]) {
                dataContainer = <VipLevel />
            } else if (TabName == this._Tabs[19]) {
                dataContainer = <PromotionReport />
            } else if (TabName == this._Tabs[20]) {
                dataContainer = <Tag onGetCustInfoDetail={this.onGetCustInfoDetail.bind(this)}/>
            } else if (TabName == this._Tabs[21]) {
                dataContainer = <WithdrawalLimitLog />
            }

            let authSubKeys = {
                10: 'ca_resetpwd',
                12: 'ca_personalMsg',
                13: 'ca_turnoverReport'
            }

            var tabBtn = this._Tabs.filter((item, index) => {
                return authSubKeys[index] === undefined || subFuncKeys.includes(authSubKeys[index]);
            }).map(function (item, index) {
                return <li className="nav-item" key={index}>
                    <a className={"nav-link" + (index == TabIdx ? " active" : "")} href="javascript:void(0);" onClick={index == TabIdx ? null : this.onTab.bind(this, index, item)}>{item}</a>
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

    ReactDOM.render(<CustInfoPanel />, document.getElementById('Panel'));
})(jQuery, React, ReactDOM, i18n, axios, moment);