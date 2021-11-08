import { Pagination } from '../commJS/Pagination.jsx';
import { number_format } from '../commJS/common';
import { PaginationTotalAmount } from '../commJS/PaginationTotalAmount.jsx';

const WithdrawalStatusOp = {
    1: <span className="status status-pending">{i18n["lbl_WithdrawalStatus1"]}</span>,
    2: <span className="status status-hold">{i18n["lbl_WithdrawalStatus2"]}</span>,
    3: <span className="status status-verify">{i18n["lbl_WithdrawalStatus3"]}</span>,
    4: <span className="status status-process">{i18n["lbl_WithdrawalStatus4"]}</span>,
    5: <span className="status status-cancelled">{i18n["lbl_WithdrawalStatus5"]}</span>,
    6: <span className="status status-approved">{i18n["lbl_WithdrawalStatus6"]}</span>,
    7: <span className="status status-rejected">{i18n["lbl_WithdrawalStatus7"]}</span>,
}

const WithdrawalStatusSelectOp = {
    1: "Pending",
    8: "To KYC",
    2: "On Hold",
    3: "Verify",
    4: "Process",
    5: "Cancelled",
    6: "Approved",
    7: "Rejected",
}

class WithdrawalInfoList extends React.Component {
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
                <td>{i18n["lbl_WithdrawalType" + row.withdrawalType]}</td>
                <td className="text-right">{number_format(row.amount)}</td>
                <td>{row.accountNo}</td>
                <td>{row.accountName}</td>
                <td>{WithdrawalStatusOp[row.status]}</td>
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
                            <th scope="col">withdrawal Type</th>
                            <th scope="col" className="text-right">Amount</th>
                            <th scope="col">To Acc. NO.</th>
                            <th scope="col">To Acc. Name</th>
                            <th scope="col">Status</th>
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

export class WithdrawalInfo extends React.Component {
    constructor(props) {
        super(props);
        this.state = { WithdrawalStatus: 0 };

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
        this.props.onChangeValue("WithdrawalInfo", { StartDate: moment(this.startDate.current.value).unix(), EndDate: moment(this.endDate.current.value).add(1, 'days').unix() - 1, Status: parseInt(this.state.WithdrawalStatus), 
                                UTCStartDate:moment.utc(this.startDate.current.value).unix(),UTCEndDate:moment.utc(this.endDate.current.value).add(1, 'days').unix()-1});
        this.props.GetWithdrawalListByCust(1);
    }

    render() {
        let withdrawalInfo = this.props.WithdrawalInfo;
        let searchData = this.props.searchData;
        let withdrawalStatusOption = null;
        if (WithdrawalStatusSelectOp) {
            withdrawalStatusOption = Object.keys(WithdrawalStatusSelectOp).map(i => {
                let selected = this.state.WithdrawalStatus === i ? "selected" : "";
                return (
                    <option key={`Withdrawal-${i}`} value={i} selected={selected}>{WithdrawalStatusSelectOp[i]}</option>
                )
            })
        }
        return (
            <div>
                <form className="filter" onSubmit={this.handleSubmit.bind(this)}>
                    <div className="input-item">
                        <span className="label">From Date</span>
                        <div className="date">
                            <input type="text" ref={this.startDate} name="StartDate" className="dateselect" required="required" data-date-end-date="0d" autoComplete="off" value={withdrawalInfo && withdrawalInfo.StartDate ? moment.unix(withdrawalInfo.StartDate).format("MM/DD/YYYY") : ""} />
                            <i className="icon-calendar dateselect" onClick={this.onStartDate.bind(this)} />
                        </div>
                    </div>
                    <div className="input-item date">
                        <span className="label">To Date</span>
                        <div className="date">
                            <input type="text" ref={this.endDate} name="EndDate" required="required" data-date-end-date="0d" autoComplete="off" value={withdrawalInfo && withdrawalInfo.EndDate ? moment.unix(withdrawalInfo.EndDate).format("MM/DD/YYYY") : ""} />
                            <i className="icon-calendar dateselect" onClick={this.onEndDate.bind(this)} />
                        </div>
                    </div>
                    <div className="input-item">
                        <span className="label">Withdrawal Status</span>
                        <div className="">
                            <select onChange={this.onChange.bind(this)} name="WithdrawalStatus">
                                <option value="0" selected="selected">All</option>
                                {withdrawalStatusOption}
                            </select>
                        </div>
                    </div>
                    <div className="button-group"><button className="btn btn-search" type="submit"><span>Search</span></button></div>
                    <div>(Adjusted to GMT+8)</div>
                </form>
                {searchData != null ? <WithdrawalInfoList searchData={searchData} ChangePage={this.props.GetWithdrawalListByCust} /> : null}
            </div>
        )
    }
}
