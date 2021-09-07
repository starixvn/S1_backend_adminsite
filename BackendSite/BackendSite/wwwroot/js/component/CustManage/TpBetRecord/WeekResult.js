import { number_format } from '../../commJS/common';
import moment from "moment-timezone";
import Filter from "./Filter";
const Result = (props) => {
    let rows = props.Datas.length === 0 ? <tr><td>No Data.</td><td /><td /><td /><td /><td /><td /><td /></tr> : props.Datas.map((d, dIdx) => {
        return (
            <tr key={`result-${dIdx}`}>
                <td>{moment(d.settledDate).format('l')}</td>
                <td>{number_format(d.totalTO)}</td>
                <td>{number_format(d.validBet)}</td>
                <td>{number_format(d.invalidBet)}</td>
                <td>{number_format(d.turnOver)}</td>
                <td>{number_format(d.runningTickets)}</td>
                <td>{number_format(d.winLoss)}</td>
                <td><button className="btn btn-add" onClick={props.ClickDetail.bind(null, d.settledDate)}>Details</button></td>
            </tr>
        )
    })
    return (
        <>
            <Filter
                FilterData={props.FilterData} 
                EditfilterData={props.EditfilterData}
                EditDate={props.EditDate}
                ClickSubmit={props.ClickSubmit}
            />
            <label>※Allow search data up to 12 months</label>
            <div className="table">
                <label class="col-form-label d-block">Statement List</label>
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th>Settled Date</th>
                            <th>Total TO</th>
                            <th>Valid Bet</th>
                            <th>Invalid Bet</th>
                            <th>Turnover (ExcludeValidTO)</th>
                            <th>Running Tickets</th>
                            <th>WinLoss</th>
                            <th>Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows}
                    </tbody>
                </table>
            </div>
        </>
    )
}

export default Result;