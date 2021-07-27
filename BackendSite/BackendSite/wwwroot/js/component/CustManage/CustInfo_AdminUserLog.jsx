import { Pagination } from '../commJS/Pagination.jsx';
import { NewPagination } from '../commJS/NewPagination.jsx';
class AdminUserLogList extends React.Component {
    constructor(props) {
        super(props);
    }

    onRefresh() {
        this.props.ChangePage(null);
    }

    render() {

        const actionTypeStatus = {
            1: "Create",
            2: "Read",
            3: "Update",
            4: "Delete",
        }

        let rows = this.props.searchData.data.map((row, idx) => {
            let datadetailInfos = JSON.parse(row.detailInfo);
            let datadetailInfo = Object.keys(datadetailInfos).map((detail, idx) => {
                return (<span key={idx}>{detail + " : " + datadetailInfos[detail]}</span>)
            })
            return (
                <tr key={"data-" + idx}>
                    <th>{row.menu}</th>
                    <td>{actionTypeStatus[row.actionType]}</td>
                    <td style={{ display: "flex", flexDirection: "column", maxWidth: "initial" }}>{datadetailInfo}</td>
                    {/* <td>{moment(row.createTime).format('l LTS')}</td> */}
                    {/* <td>{row.createTime}</td> */}
                    <td>{moment.utc(row.createTime).add(8,'hour').format('l LTS')}</td>
                    <td>{row.operator}</td>
                </tr>
            );
        });

        return (
            <div className="table" >
                <div className="table-options">
                    <button type="button" className="btn btn-refresh" onClick={this.onRefresh.bind(this)}>
                        Refresh
                    </button>
                </div>
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th scope="col">Menu</th>
                            <th scope="col">ActionType</th>
                            <th scope="col">DetailInfo</th>
                            {/* <th scope="col">Create Time {moment().format('(Z)')}</th> */}
                            <th scope="col">Create Time (Adjusted to GMT+8)</th>
                            <th scope="col">Operator</th>
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

export class AdminUserLog extends React.Component {
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
        this.props.onChangeValue("AdminUserLog", { StartDate: moment(this.startDate.current.value).unix(), EndDate: moment(this.endDate.current.value).add(1, 'days').unix() - 1,
                                UTCStartDate:moment.utc(this.startDate.current.value).unix(),UTCEndDate:moment.utc(this.endDate.current.value).add(1, 'days').unix()-1 });
        this.props.GetAdminUserLogByCust(1);
    }

    render() {
        let adminUserLog = this.props.AdminUserLog;
        let searchData = this.props.searchData;
        return (
            <div>
                <form className="filter" onSubmit={this.handleSubmit.bind(this)}>
                    <div className="input-item">
                        <span className="label">From Date</span>
                        <div className="date">
                            <input type="text" ref={this.startDate} name="StartDate" className="dateselect" required="required" data-date-end-date="0d" autoComplete="off" value={adminUserLog && adminUserLog.StartDate ? moment.unix(adminUserLog.StartDate).format("MM/DD/YYYY") : ""} />
                            <i className="icon-calendar dateselect" onClick={this.onStartDate.bind(this)} />
                        </div>
                    </div>
                    <div className="input-item date">
                        <span className="label">To Date</span>
                        <div className="date">
                            <input type="text" ref={this.endDate} name="EndDate" required="required" data-date-end-date="0d" autoComplete="off" value={adminUserLog && adminUserLog.EndDate ? moment.unix(adminUserLog.EndDate).format("MM/DD/YYYY") : ""} />
                            <i className="icon-calendar dateselect" onClick={this.onEndDate.bind(this)} />
                        </div>
                    </div>
                    <div className="button-group"><button className="btn btn-search" type="submit"><span>Search</span></button></div>
                    <div>(Adjusted to GMT+8)</div>
                </form>
                {searchData != null ? <AdminUserLogList searchData={searchData} ChangePage={this.props.GetAdminUserLogByCust} /> : null}
            </div>
        )
    }
}
