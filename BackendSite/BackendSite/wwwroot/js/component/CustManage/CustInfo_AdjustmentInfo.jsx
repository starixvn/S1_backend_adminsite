import { Pagination } from '../commJS/Pagination.jsx';
import { number_format } from '../commJS/common';
import { PaginationTotalAmount } from '../commJS/PaginationTotalAmount.jsx';

class AdjustmentInfoList extends React.Component {
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
        const AdjustmentInfoStatus = {
            1: "Pending",
            2: "Approved",
            3: "Rejected"
        }

        const Direction = {
            1: "+",
            2: "-",
            3: "+",
            4: "-"
        }

        let rows = this.props.searchData.data.map(row =>
            <tr key={row.transId}>
                <td>{moment.utc(row.createTime).add(8,'hour').format('l LTS')}</td>
                <th scope="row">{row.transId}</th>
                <td>{i18n["lbl_Currency" + row.currencyId]}</td>
                <td className="text-right">{Direction[row.adjustType] + number_format(row.amount)}</td>
                <td>{AdjustmentInfoStatus[row.status]}</td>
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
                            <th scope="col">CreateTime</th>
                            <th scope="col">TransId</th>
                            <th scope="col">Currency</th>
                            <th scope="col" className="text-right">Amount</th>
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

export class AdjustmentInfo extends React.Component {
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

    handleSubmit(event) {
        event.preventDefault();
        this.props.onChangeValue("AdjustmentInfo", { StartDate: moment.utc(this.startDate.current.value).add(-8,'hour').unix(), EndDate: moment.utc(this.endDate.current.value).add(-8,'hour').add(1, 'days').unix() - 1 });
        this.props.GetAdjustmentInfoByCust(1);
    }

    render() {
        let adjustmentInfo = this.props.AdjustmentInfo;
        let searchData = this.props.searchData;
        return (
            <div>
                <form className="filter" onSubmit={this.handleSubmit.bind(this)}>
                    <div className="input-item">
                        <span className="label">From Date</span>
                        <div className="date">
                            <input type="text" ref={this.startDate} name="StartDate" className="dateselect" required="required" data-date-end-date="0d" autoComplete="off" value={adjustmentInfo && adjustmentInfo.StartDate ? moment.unix(adjustmentInfo.StartDate).format("MM/DD/YYYY") : ""} />
                            <i className="icon-calendar dateselect" onClick={this.onStartDate.bind(this)} />
                        </div>
                    </div>
                    <div className="input-item date">
                        <span className="label">To Date</span>
                        <div className="date">
                            <input type="text" ref={this.endDate} name="EndDate" required="required" data-date-end-date="0d" autoComplete="off" value={adjustmentInfo && adjustmentInfo.EndDate ? moment.unix(adjustmentInfo.EndDate).format("MM/DD/YYYY") : ""} />
                            <i className="icon-calendar dateselect" onClick={this.onEndDate.bind(this)} />
                        </div>
                    </div>
                    <div className="button-group"><button className="btn btn-search" type="submit"><span>Search</span></button></div>
                </form>
                {searchData != null ? <AdjustmentInfoList searchData={searchData} ChangePage={this.props.GetAdjustmentInfoByCust} /> : null}
            </div>
        )
    }
}
