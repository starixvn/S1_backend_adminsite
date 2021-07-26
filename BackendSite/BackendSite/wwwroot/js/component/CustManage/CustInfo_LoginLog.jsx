import {Pagination} from '../commJS/Pagination.jsx';
import { NewPagination } from '../commJS/NewPagination.jsx';
import moment from 'moment';
class LoginLogList extends React.Component {
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
            <tr key={row.Id}>
                <td>{moment.utc(row.loginTime).add(8,'hour').format('l LTS')}</td>                
                <th scope="row">{row.loginIp}</th>
                <td>{row.fromUrl}</td>
                <td>{row.userAgent}</td>
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
                            <th scope="col">Login Time</th>                            
                            <th scope="col">Login IP</th>
                            <th scope="col">From Url</th>
                            <th scope="col">User Agent</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows}
                    </tbody>
                </table>
                <NewPagination Data={{...this.props.searchData}} ChangePage={this.props.ChangePage}/>
            </div>
        )
    }
}

export class LoginLog extends React.Component {
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
        this.props.onChangeValue("LoginLog", {StartDate: moment(this.startDate.current.value).unix(), EndDate: moment(this.endDate.current.value).add(1, 'days').unix()-1,
                                UTCStartDate:moment.utc(this.startDate.current.value).unix(),UTCEndDate:moment.utc(this.endDate.current.value).add(1, 'days').unix()-1});
        this.props.GetLoginLogByCust(1);
    }

    render() {
        let loginLog = this.props.LoginLog;
        let searchData = this.props.searchData;
        return (
            <div>
                <form className="filter" onSubmit={this.handleSubmit.bind(this)}>
                    <div className="input-item">
                        <span className="label">From Date</span>
                        <div className="date">
                            <input type="text" ref={this.startDate} name="StartDate" className="dateselect" required="required" data-date-end-date="0d" autoComplete="off" value={loginLog&&loginLog.StartDate ? moment.unix(loginLog.StartDate).format("MM/DD/YYYY"):""} />
                            <i className="icon-calendar dateselect" onClick={this.onStartDate.bind(this)} />
                        </div>                            
                    </div>     
                    <div className="input-item date">
                        <span className="label">To Date</span>
                        <div className="date">
                            <input type="text" ref={this.endDate} name="EndDate" required="required" data-date-end-date="0d" autoComplete="off" value={loginLog&&loginLog.EndDate ? moment.unix(loginLog.EndDate).format("MM/DD/YYYY"):""}/>
                            <i className="icon-calendar dateselect" onClick={this.onEndDate.bind(this)} />
                        </div>                            
                    </div>
                    <div className="button-group"><button className="btn btn-search" type="submit"><span>Search</span></button></div>
                </form>
                {searchData!=null ? <LoginLogList searchData={searchData} ChangePage={this.props.GetLoginLogByCust} /> : null}
            </div>
        )
    }
}
