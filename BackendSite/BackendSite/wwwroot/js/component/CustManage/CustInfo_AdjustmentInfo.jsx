import {Pagination} from '../commJS/Pagination.jsx';
import {number_format} from '../commJS/common';
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
        let rows = this.props.searchData.data.map(row => 
            <tr key={row.transId}>
                <td>{moment(row.createTime).format('l LTS')}</td>                
                <th scope="row">{row.transId}</th>
                <td>{row.tpName}</td>
                <td>{row.direction}</td>                
                <td className="text-right">{number_format(row.amount)}</td>                    
                <td>{row.thirdId}</td>
                <td>{row.Memo}</td>
                <td>{row.status}</td>
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
                            <th scope="col">Third-Party Name</th>
                            <th scope="col">Direction</th>                            
                            <th scope="col" className="text-right">Amount</th>
                            <th scope="col">Third-Party ID</th>
                            <th scope="col">Memo</th>                            
                            <th scope="col">Status</th>
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

export class AdjustmentInfo extends React.Component {
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
        this.props.onChangeValue("TpTransferInfo", {StartDate: moment(this.startDate.current.value).unix(), EndDate: moment(this.endDate.current.value).add(1, 'days').unix()-1});
        this.props.GetTpTransferInfoByCust(1);
    }

    render() {
        let tpTransferInfo = this.props.TpTransferInfo;
        let searchData = this.props.searchData;
        return (
            <div>
                <form className="filter" onSubmit={this.handleSubmit.bind(this)}>
                    <div className="input-item">
                        <span className="label">From Date</span>
                        <div className="date">
                            <input type="text" ref={this.startDate} name="StartDate" className="dateselect" required="required" data-date-end-date="0d" autoComplete="off" value={tpTransferInfo&&tpTransferInfo.StartDate ? moment.unix(tpTransferInfo.StartDate).format("MM/DD/YYYY"):""} />
                            <i className="icon-calendar dateselect" onClick={this.onStartDate.bind(this)} />
                        </div>                            
                    </div>     
                    <div className="input-item date">
                        <span className="label">To Date</span>
                        <div className="date">
                            <input type="text" ref={this.endDate} name="EndDate" required="required" data-date-end-date="0d" autoComplete="off" value={tpTransferInfo&&tpTransferInfo.EndDate ? moment.unix(tpTransferInfo.EndDate).format("MM/DD/YYYY"):""}/>
                            <i className="icon-calendar dateselect" onClick={this.onEndDate.bind(this)} />
                        </div>                            
                    </div>
                    <div className="button-group"><button className="btn btn-search" type="submit"><span>Search</span></button></div>
                </form>
                {searchData!=null ? <AdjustmentInfoList searchData={searchData} ChangePage={this.props.GetTpTransferInfoByCust} /> : null}
            </div>
        )
    }
}
