import React from "react";
import { NewPagination } from '../commJS/NewPagination.jsx';
import moment from 'moment';
import { number_format } from '../commJS/common';

class TpAdjustmentInfoList extends React.Component {
    constructor(props) {
        super(props);
    }

    onRefresh() {
        this.props.ChangePage(null);
    }

    render() {
        const StatusData = {
            0: "Success",
            2: "Fail"
        }

        const AdjustmentOptionData = {
            1: "Add",
            2: "Minus"
        }

        let searchData = this.props.searchData.data;
        console.log(searchData);
        let dataRows = searchData.length === 0 ? <tr></tr> : searchData.map((data, idx) => {
            return (
                <tr key={`result-body-${idx}`}>
                    <td>{data.siteName}</td>
                    <td>{data.tpName}</td>
                    <td>{data.transId}</td>
                    <td>{AdjustmentOptionData[data.direction]}</td>
                    <td>{number_format(data.amount, 2)}</td>
                    <td>{StatusData[data.status]}</td>
                    <td>{data.categoryName}</td>
                    <td>{data.memo}</td>
                    <td>{data.operator}</td>
                    <td>{data.remarks}</td>
                    <td>{moment(data.createTime).format('l LTS')}</td>
                </tr>
            )
        });

        let headerRows = searchData.length === 0 ? null : (
            <>
                <th scope="col">Site Name</th>
                <th scope="col">3RD Name</th>
                <th scope="col">Trans Id</th>
                <th scope="col">Adjust Type</th>
                <th scope="col">Adjust Amount</th>
                <th scope="col">Status</th>
                <th scope="col">Adjust Reason</th>
                <th scope="col">Trans Memo</th>
                <th scope="col">Operator</th>
                <th scope="col">Remarks</th>
                <th scope="col">Create Time</th>
            </>
        );

        return (
            <div className="table">
                <table className="table table-striped">
                    <thead>
                        <tr>
                            {headerRows}
                        </tr>
                    </thead>
                    <tbody>
                        {dataRows}
                    </tbody>
                </table>
                {
                    searchData.length === 0 ? null : (
                        <NewPagination Data={{...this.props.searchData}} ChangePage={this.props.ChangePage}/>
                    )
                }
            </div>
        )
    }
}

export class TpAdjustmentInfo extends React.Component {
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
        var startTime = moment(this.startDate.current.value);
        var endTime = moment(this.endDate.current.value).add(1, 'days');
        
        event.preventDefault();
        this.props.onChangeValue("TpAdjustmentInfo", { StartDate: startTime.unix(), EndDate: endTime.unix() - 1,
            UTCStartDate: startTime.format('YYYY-MM-DDThh:mm:ssZ'), UTCEndDate: endTime.format('YYYY-MM-DDThh:mm:ssZ') });
        this.props.GetTpAdjustmentInfoByCust(1);
    }

    render() {
        let adjustmentInfo = this.props.TpAdjustmentInfo;
        let searchData = this.props.searchData;
        console.log(this.props, "1234688");
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
                    <div>(Adjusted to GMT+8)</div>
                </form>
                {searchData != null ? <TpAdjustmentInfoList searchData={searchData} ChangePage={this.props.GetTpAdjustmentInfoByCust} /> : null}
            </div>
        )
    }
}