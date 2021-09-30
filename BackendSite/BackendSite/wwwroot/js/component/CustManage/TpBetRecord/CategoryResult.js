import { number_format } from '../../commJS/common';
import moment from "moment-timezone";
const Result = (props) => {
    let rows = props.Datas.length === 0 ? <tr><td>No Data.</td><td /><td /><td /><td /><td /><td /></tr> : props.Datas.map((d, dIdx) => {
        return (
            <tr key={`result-${dIdx}`}>
                <td>{moment(d.settledDate).format('l')}</td>
                <td>{d.gameTypeName}</td>
                <td>{number_format(d.totalTO)}</td>
                <td>{number_format(d.validBet)}</td>
                <td>{number_format(d.invalidBet)}</td>
                <td>{number_format(d.turnOver)}</td>
                <td>{number_format(d.runningTickets)}</td>
                <td>{number_format(d.winLoss)}</td>
                <td><button className="btn btn-add" onClick={props.ClickDetail.bind(null, d.settledDate, d.gameTypeID, 1)}>Details</button></td>
            </tr>
        )
    })
    return (
        <>
            <div className="table">
                <label class="col-form-label d-block">Statement By Category</label>
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th>Settled Date</th>
                            <th>Game Type Name</th>
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
                <button type="button" className="btn btn-back" onClick={props.ClickBack.bind(null, "day")} style={{ marginTop: '1rem' }}>Back</button>
            </div>
        </>
    )
}

export default Result;